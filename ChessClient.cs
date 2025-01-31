﻿using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace chess
{
    enum MessageType
    {
        Ack,

        RequestMatch,
        CancelMatch,
        AcceptMatch,
        RejectMatch,

        MovePiece,

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

    struct RequestMatchPayload
    {
        public Profile profile;

        public byte[] Encode()
        {
            return profile.Encode();
        }

        public static RequestMatchPayload Decode(byte[] payload)
        {
            return new RequestMatchPayload { profile = Profile.Decode(payload) };
        }
    }

    struct AcceptMatchPayload
    {
        public Profile profile;

        public byte[] Encode()
        {
            return profile.Encode();
        }

        public static RequestMatchPayload Decode(byte[] payload)
        {
            return new RequestMatchPayload { profile = Profile.Decode(payload) };
        }
    }

    struct MovePiecePayload
    {
        public byte pieceId;
        public byte x, y;

        public byte[] Encode()
        {
            var payload = new byte[3];
            payload[0] = pieceId;
            payload[1] = x;
            payload[2] = y;
            return payload;
        }

        public static MovePiecePayload Decode(byte[] payload)
        {
            return new MovePiecePayload {
                pieceId = payload[0],
                x = payload[1],
                y = payload[2]
            };
        }
    }

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
        public MessageType type;
        public UInt16 id;
    };

    public class SentMatchRequest
    {
        public bool received = false;
        public bool rejected = false;
        // TODO: public bool timeout = false;
        
        public EndPoint remote;
    };

    public class ReceivedMatchRequest
    {
        public bool cancelled = false;

        public Profile profile;
        public EndPoint remote;
    };

    public class EnemyInfo
    {
        public bool isWhite = false;
        public bool acceptedMatch = false;
        public EndPoint? endpoint = null;
        public Profile profile;

        // TODO:
        // public uint ping;
    };

    public class PieceMoved
    {
        public int pieceId;
        public int x, y;
    }

    internal class ChessClient
    {
        UInt16 lastMessageId = 0;

        List<AckableMessage> unackedMessages;
        // TODO: TimeSpan retransmitInterval = TimeSpan.FromSeconds(1);
        Socket socket;

        public EnemyInfo enemyInfo;
        PieceMoved? enemyMove;

        DateTime? lastCursorSentAt;
        Vector2 targetEnemyPos;
        public PlayerCursor myCursor;
        public PlayerCursor enemyCursor;
        public float cursorUpdateRate = 20; // Updates per second
        public float cursorLerpStrength = 25;

        public SentMatchRequest? sentMatchRequest = null;
        public ReceivedMatchRequest? receivedMatchRequest = null;

        public ChessClient(PlayerCursor myCursor, PlayerCursor enemyCursor, UInt16 port = 8080)
        {
            this.myCursor = myCursor;
            this.enemyCursor = enemyCursor;

            targetEnemyPos = enemyCursor.pos;

            enemyInfo = new EnemyInfo();
            unackedMessages = new List<AckableMessage>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Blocking = false;

            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch (SocketException)
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            }
        }

        public static bool IsPortUsed(UInt16 port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
            } catch (SocketException)
            {
                return true;
            } finally
            {
                socket.Close();
            }

            return false;
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

                targetEnemyPos = new Vector2(payload.x, payload.y);
                enemyCursor.chessPiece = payload.chessPieceId;
                enemyCursor.type = payload.type;
            }
            else if (msg.type == MessageType.RequestMatch)
            {
                if (receivedMatchRequest == null)
                {
                    var payload = RequestMatchPayload.Decode(msg.payload);

                    receivedMatchRequest = new ReceivedMatchRequest();
                    receivedMatchRequest.remote = msg.remote;
                    receivedMatchRequest.profile = payload.profile;
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
            else if (msg.type == MessageType.AcceptMatch)
            {
                if (sentMatchRequest != null)
                {
                    var payload = AcceptMatchPayload.Decode(msg.payload);

                    enemyInfo.endpoint = sentMatchRequest.remote;
                    enemyInfo.profile = payload.profile;
                    enemyInfo.acceptedMatch = true;
                    enemyInfo.isWhite = false;

                    sentMatchRequest = null;
                }
            }
            else if (msg.type == MessageType.RejectMatch)
            {
                if (sentMatchRequest != null)
                {
                    sentMatchRequest.rejected = true;
                    sentMatchRequest = null;
                }
            }
            else if (msg.type == MessageType.MovePiece)
            {
                var payload = MovePiecePayload.Decode(msg.payload);
                enemyMove = new PieceMoved();
                enemyMove.pieceId = payload.pieceId;
                enemyMove.x = payload.x;
                enemyMove.y = payload.y;
            }
        }

        void OnMessageAcked(AckableMessage msg)
        {
            if (msg.type == MessageType.RequestMatch)
            {
                if (sentMatchRequest != null)
                {
                    sentMatchRequest.received = true;
                } 
            }
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
                    type = type,
                    sentAt = DateTime.Now
                });
            }
        }

        ReceivedMessage? ReceiveMessage()
        {
            byte[] packet = new byte[255 + 2];
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

            int packetLength;
            try
            {
                packetLength = socket.ReceiveFrom(packet, ref remote);
            } catch (SocketException)
            {
                return null;
            }

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

        public void SendMatchRequest(Profile myProfile, IPEndPoint remote)
        {
            Debug.Assert(sentMatchRequest == null);

            var matchRequest = new SentMatchRequest();
            matchRequest.remote = remote;

            var payload = new RequestMatchPayload{ profile = myProfile };
            SendMessage(remote, MessageType.RequestMatch, payload.Encode());

            sentMatchRequest = matchRequest;
        }

        public void CancelMatchRequest()
        {
            if (sentMatchRequest == null) return;

            SendMessage(sentMatchRequest.remote, MessageType.CancelMatch, null);

            sentMatchRequest = null;
        }

        public void AcceptMatchRequest(Profile profile)
        {
            if (receivedMatchRequest == null) return;

            var payload = new AcceptMatchPayload { profile = profile };
            SendMessage(receivedMatchRequest.remote, MessageType.AcceptMatch, payload.Encode());
            enemyInfo.endpoint = receivedMatchRequest.remote;
            enemyInfo.profile = receivedMatchRequest.profile;
            enemyInfo.acceptedMatch = true;
            enemyInfo.isWhite = true;

            receivedMatchRequest = null;
        }

        public void MovePiece(int pieceId, int x, int y)
        {
            if (enemyInfo.endpoint == null) return;

            Debug.Assert(0 <= pieceId && pieceId <= 255);
            Debug.Assert(0 <= x && x <= 255);
            Debug.Assert(0 <= y && y <= 255);

            var payload = new MovePiecePayload
            {
                pieceId = (byte)pieceId,
                x = (byte)x,
                y = (byte)y
            };
            SendMessage(enemyInfo.endpoint, MessageType.MovePiece, payload.Encode());
        }

        public PieceMoved? GetEnemyMove()
        {
            var move = enemyMove;
            if (move != null)
            {
                enemyMove = null;
            }
            return move;
        }

        public void RejectMatchRequest()
        {
            if (receivedMatchRequest == null) return;

            SendMessage(receivedMatchRequest.remote, MessageType.RejectMatch, null);

            receivedMatchRequest = null;
        }

        public void Update(float dt)
        {
            if (enemyInfo.endpoint != null)
            {
                var cursorUpdateInterval = TimeSpan.FromSeconds(1 / cursorUpdateRate);
                var now = DateTime.Now;
                if (lastCursorSentAt == null)
                {
                    lastCursorSentAt = now.Subtract(cursorUpdateInterval);
                }

                if (now.Subtract(lastCursorSentAt.Value) > cursorUpdateInterval)
                {
                    var payload = CursorPayload.FromPlayerCursor(myCursor);
                    SendMessage(enemyInfo.endpoint, MessageType.Cursor, payload.Encode());
                    lastCursorSentAt = now;
                }

                enemyCursor.pos = Vector2.Lerp(enemyCursor.pos, targetEnemyPos, cursorLerpStrength * dt);
            }

            if (socket.Poll(0, SelectMode.SelectRead))
            {
                var receivedMessage = ReceiveMessage();
                if (receivedMessage != null)
                {
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
