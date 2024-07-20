using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace chess
{
    enum MessageType
    {
        Ack,

        RequestMatch,
        CancelMatch,
        AcceptMatch,
        RejectMatch,

        Cursor
    };

    struct CursorPayload
    {
        public float x, y;
        public byte chessPieceId;
        public CursorType type;

        public static CursorPayload FromPlayerCursor(PlayerCursor playerCursor)
        {
            Debug.Assert(0 <= playerCursor.chessPiece && playerCursor.chessPiece <= 255);

            return new CursorPayload
            {
                x = playerCursor.pos.X,
                y = playerCursor.pos.Y,
                chessPieceId = (byte)playerCursor.chessPiece,
                type = playerCursor.type,
            };
        }

        public byte[] Encode()
        {
            byte[] payload = new byte[1 + 4 + 4 + 1];
            BitConverter.GetBytes(x).CopyTo(payload, 0);
            BitConverter.GetBytes(y).CopyTo(payload, 4);
            payload[8] = chessPieceId;
            payload[9] = (byte)type;
            return payload;
        }

        public static CursorPayload Decode(byte[] payload)
        {
            return new CursorPayload
            {
                x = BitConverter.ToSingle(payload, 0),
                y = BitConverter.ToSingle(payload, 4),
                chessPieceId = payload[8],
                type = (CursorType)payload[9],
            };
        }
    };

    class ReceivedMessage
    {
        public EndPoint remote;
        public UInt16? id = null;
        public MessageType type;
        public byte[] payload;
    };

    class AckableMessage
    {
        public bool acked = false;
        public DateTime sentAt;

        public EndPoint remote;
        public UInt16 id;
    };

    public class MatchRequest
    {
        public EndPoint remote;
        public bool cancelled = false;
    };

    internal class ChessClient
    {
        UInt16 lastMessageId = 0;

        List<AckableMessage> unackedMessages;
        // TODO: TimeSpan retransmitInterval = TimeSpan.FromSeconds(1);
        Socket socket;

        public EndPoint? enemyEndpoint;

        DateTime? lastCursorSentAt;
        public PlayerCursor myCursor;
        public PlayerCursor enemyCursor;
        public float cursorUpdateRate = 10; // Updates per second

        MatchRequest? sentMatchRequest = null;
        MatchRequest? receivedMatchRequest = null;

        public ChessClient(PlayerCursor myCursor, PlayerCursor enemyCursor, int port = 8080)
        {
            this.myCursor = myCursor;
            this.enemyCursor = enemyCursor;

            unackedMessages = new List<AckableMessage>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Blocking = false;

            var address = IPAddress.Any;
            try
            {
                socket.Bind(new IPEndPoint(address, port));
            }
            catch (SocketException)
            {
                socket.Bind(new IPEndPoint(address, 0));
            }
        }

        public UInt16 getPort()
        {
            return (UInt16)(this.socket.LocalEndPoint as IPEndPoint).Port;
        }

        static bool doesMessageNeedAck(MessageType messageType)
        {
            MessageType[] noNeedsAcks =
            {
                MessageType.Ack,
                MessageType.Cursor
            };

            foreach (var noNeedsAck in noNeedsAcks)
            {
                if (noNeedsAck == messageType)
                {
                    return false;
                }
            }

            return true;
        }

        void OnMessageReceived(ReceivedMessage msg)
        {
            if (msg.type == MessageType.Cursor)
            {
                var payload = CursorPayload.Decode(msg.payload);

                enemyCursor.pos = new Vector2(payload.x, payload.y);
                enemyCursor.chessPiece = payload.chessPieceId;
                enemyCursor.type = payload.type;
            }
            else if (msg.type == MessageType.AcceptMatch)
            {
                if (receivedMatchRequest == null)
                {
                    receivedMatchRequest = new MatchRequest();
                    receivedMatchRequest.remote = msg.remote;
                }
            }
            else if (msg.type == MessageType.CancelMatch)
            {
                if (receivedMatchRequest != null)
                {
                    receivedMatchRequest.cancelled = true;
                    receivedMatchRequest = null;
                }
            }
        }

        void OnMessageAcked(AckableMessage msg)
        {
        }

        void SendMessage(EndPoint remote, MessageType type, Span<byte> payload)
        {
            UInt16? id = null;
            bool needsAck = doesMessageNeedAck(type);

            int messageSize = 0;
            messageSize += 1; // Type
            messageSize += 1; // Length
            if (needsAck)
            {
                messageSize += 2; // Id
            }
            messageSize += payload.Length;

            byte[] messageBytes = new byte[messageSize];
            messageBytes[0] = (byte)type;
            messageBytes[1] = (byte)payload.Length;
            if (needsAck)
            {
                id = lastMessageId++;

                Debug.Assert(payload.Length < 255 - 2);
                messageBytes[1] += 2;
                BitConverter.GetBytes(id.Value).CopyTo(messageBytes, 2);

                for (int i = 0; i < payload.Length; i++)
                {
                    messageBytes[i + 4] = payload[i];
                }
            }
            else
            {
                for (int i = 0; i < payload.Length; i++)
                {
                    messageBytes[i + 2] = payload[i];
                }
            }

            try
            {
                socket.SendTo(messageBytes, remote);
            } catch (SocketException)
            {
                return;
            }

            if (needsAck)
            {
                unackedMessages.Add(new AckableMessage
                {
                    remote = remote,
                    id = id.Value,
                    sentAt = DateTime.Now
                });
            }
        }

        ReceivedMessage ReceiveMessage()
        {
            byte[] packet = new byte[255 + 2];
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            int packetLength = socket.ReceiveFrom(packet, ref remote);

            MessageType messageType = (MessageType)packet[0];
            byte messageLength = packet[1];

            Debug.Assert(messageLength == packetLength - 2);

            bool needsAck = doesMessageNeedAck(messageType);
            byte[] payload;
            UInt16? id = null;
            if (needsAck || messageType == MessageType.Ack)
            {
                id = BitConverter.ToUInt16(packet, 2);
                payload = packet.Skip(4).ToArray();
            }
            else
            {
                payload = packet.Skip(2).ToArray();
            }

            if (needsAck)
            {
                Debug.Assert(id != null);

                byte[] messageBytes = new byte[1 + 1 + 2];
                messageBytes[0] = (byte)MessageType.Ack;
                messageBytes[1] = 2;
                BitConverter.GetBytes(id.Value).CopyTo(messageBytes, 2);
                socket.SendTo(messageBytes, remote);
            }

            return new ReceivedMessage{
                type = messageType,
                remote = remote,
                id = id,
                payload = payload
            };
        }

        public MatchRequest SendMatchRequest(IPEndPoint remote)
        {
            Debug.Assert(sentMatchRequest == null);

            var matchRequest = new MatchRequest();

            SendMessage(remote, MessageType.RequestMatch, null);

            sentMatchRequest = matchRequest;

            return matchRequest;
        }

        public void CancelMatchRequest(IPEndPoint remote)
        {
            SendMessage(remote, MessageType.CancelMatch, null);

            sentMatchRequest = null;
        }

        public void AcceptMatchRequest()
        {
            if (receivedMatchRequest == null) return;

            SendMessage(receivedMatchRequest.remote, MessageType.AcceptMatch, null);

            receivedMatchRequest = null;
        }

        public void RejectMatchRequest()
        {
            if (receivedMatchRequest == null) return;

            SendMessage(receivedMatchRequest.remote, MessageType.RejectMatch, null);

            receivedMatchRequest = null;
        }

        public void Update()
        {
            if (enemyEndpoint != null)
            {
                var cursorUpdateInterval = TimeSpan.FromSeconds(1 / cursorUpdateRate);
                var now = DateTime.Now;
                if (lastCursorSentAt == null)
                {
                    lastCursorSentAt = now.Subtract(TimeSpan.FromSeconds(1 / cursorUpdateRate));
                }

                if (now.Subtract(lastCursorSentAt.Value) > cursorUpdateInterval)
                {
                    var payload = CursorPayload.FromPlayerCursor(myCursor);
                    SendMessage(enemyEndpoint, MessageType.Cursor, payload.Encode());
                }
            }

            if (socket.Available > 0)
            {
                var receivedMessage = ReceiveMessage();

                if (receivedMessage.type == MessageType.Ack) {
                    foreach (var msg in unackedMessages)
                    {
                        if (msg.id == receivedMessage.id)
                        {
                            msg.acked = true;
                            OnMessageAcked(msg);
                            break;
                        }
                    }

                } else {

                    OnMessageReceived(receivedMessage);
                }
            }
            
            // TODO: Add retransmittion to messages which were not acked

            for (int i = 0; i < unackedMessages.Count; i++)
            {
                AckableMessage msg = unackedMessages[i];
                if (msg.acked)
                {
                    unackedMessages.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
