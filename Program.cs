using Raylib_CsLo;
using System.Drawing;
using System.Net;
using System.Numerics;
using Rectangle = Raylib_CsLo.Rectangle;

namespace chess;

class Program
{
    public static List<Chesspiece> chesspieces = new List<Chesspiece>();
    public static PlayerCursor myCursor = new PlayerCursor(Vector2.Zero, 0, CursorType.Default);
    public static PlayerCursor enemyCursor = new PlayerCursor(Vector2.Zero, 0, CursorType.Default);
    public static Profile myProfile = new Profile("<unknown>", 0, 0);
    public static ChessClient net;

    // Main menu specific variables
    public static bool editConnectIp = false;
    public static sbyte[] connectIp = new sbyte[16];
    public static bool editConnectPort = false;
    public static sbyte[] connectPort = new sbyte[16];

    // First time setup specific variables
    public static bool firstTimeSetup = false;
    public static bool editName = false;
    public static sbyte[] name = new sbyte[32];

    public static void Main()
    {
        UInt16 myPort = 8080;
        UInt16 enemyPort = 8082;

        if (true)
        {
            if (ChessClient.IsPortUsed(myPort))
            {
                (myPort, enemyPort) = (enemyPort, myPort);
            }

            CopyString("127.0.0.1", connectIp);
            CopyString($"{enemyPort}", connectPort);
        }

        if (!myProfile.Load())
        {
            firstTimeSetup = true;
        }

        net = new ChessClient(myCursor, enemyCursor, myPort);
        net.enemyEndpoint = new IPEndPoint(IPAddress.Loopback, enemyPort);

        {
            int i = 0;
            chesspieces.Add(new Chesspiece(i++, new Vector2(0, 0), PieceType.Rook, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(1, 0), PieceType.Knight, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(2, 0), PieceType.Bishop, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(7, 0), PieceType.Rook, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(6, 0), PieceType.Knight, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(5, 0), PieceType.Bishop, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(0, 1), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(1, 1), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(2, 1), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(3, 1), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(4, 1), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(5, 1), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(6, 1), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(7, 1), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(3, 0), PieceType.King, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(4, 0), PieceType.Queen, true));

            chesspieces.Add(new Chesspiece(i++, new Vector2(0, 7), PieceType.Rook, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(1, 7), PieceType.Knight, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(2, 7), PieceType.Bishop, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(7, 7), PieceType.Rook, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(6, 7), PieceType.Knight, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(5, 7), PieceType.Bishop, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(0, 6), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(1, 6), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(2, 6), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(3, 6), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(4, 6), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(5, 6), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(6, 6), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(7, 6), PieceType.Pawn, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(3, 7), PieceType.King, true));
            chesspieces.Add(new Chesspiece(i++, new Vector2(4, 7), PieceType.Queen, true));
        }

        Raylib.SetTargetFPS(144);
        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(1000, 1000, "Chess innit");

        RayGui.GuiLoadStyleDefault();
        RayGui.GuiSetStyle(0, 16, 30);
        RayGui.GuiSetStyle(0, 20, 3);

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            net.Update(dt);

            if (firstTimeSetup)
            {
                ShowFirstTimeSetup(dt);
            } else if (net.GetEnemy() == null)
            {
                ShowMainMenuScreen(dt);
            } else
            {
                ShowBoardScreen(dt);
            }
        }

        Raylib.CloseWindow();
    }

    public static void CopyString(string from, sbyte[] to)
    {
        for (int i = 0; i < from.Length; i++)
        {
            to[i] = (sbyte)from[i];
        }
        to[from.Length] = 0;
    }

    public static string ConvertToStringFrombytes(sbyte[] bytes)
    {
        var chars = new char[bytes.Length];
        var strLength = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            chars[i] = (char)bytes[i];
            if (bytes[i] == 0) break;
            strLength++;
        }

        return new string(chars, 0, strLength);
    }

    public static void ShowFirstTimeSetup(float dt)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.WHITE);

        var windowRect = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        var stack = new VerticalStack
        {
            gap = 20,
            position = RectUtils.GetCenteredPosition(windowRect, new Vector2(200, 300))
        };

        RayGui.GuiLabel(stack.nextRectangle(100, 50), $"Name");

        unsafe
        {
            fixed (sbyte* namePtr = name)
            {
                if (RayGui.GuiTextBox(stack.nextRectangle(200, 50), namePtr, 30, editName))
                {
                    editName = !editName;
                }
            }
        }

        var nameStr = ConvertToStringFrombytes(name);
        if (nameStr.Length > 0)
        {
            if (RayGui.GuiButton(stack.nextRectangle(150, 50), "Continue"))
            {
                firstTimeSetup = false;
                myProfile.name = nameStr;
                myProfile.Save();
            }
        }

        Raylib.EndDrawing();
    }

    public static void ShowMainMenuScreen(float dt)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.WHITE);

        var windowRect = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        if (net.receivedMatchRequest != null)
        {
            var stack = new VerticalStack
            {
                gap = 20,
                position = RectUtils.GetCenteredPosition(windowRect, new Vector2(200, 300))
            };

            RayGui.GuiLabel(stack.nextRectangle(100, 50), $"Got match request");

            if (RayGui.GuiButton(stack.nextRectangle(150, 50), "Accept"))
            {
                net.AcceptMatchRequest();
            }

            if (RayGui.GuiButton(stack.nextRectangle(150, 50), "Reject"))
            {
                net.RejectMatchRequest();
            }

        } else
        {
            var stack = new VerticalStack
            {
                gap = 20,
                position = RectUtils.GetCenteredPosition(windowRect, new Vector2(200, 300))
            };

            RayGui.GuiLabel(stack.nextRectangle(100, 50), $"Listening on port {net.getPort()}");

            unsafe
            {
                fixed (sbyte* connectIpPtr = connectIp)
                {
                    if (RayGui.GuiTextBox(stack.nextRectangle(200, 50), connectIpPtr, 30, editConnectIp))
                    {
                        editConnectIp = !editConnectIp;
                    }
                }

                fixed (sbyte* connectPortPtr = connectPort)
                {
                    if (RayGui.GuiTextBox(stack.nextRectangle(100, 50), connectPortPtr, 30, editConnectPort))
                    {
                        editConnectPort = !editConnectPort;
                    }
                }
            }

            connectIp[connectIp.Length - 1] = 0;
            connectPort[connectPort.Length - 1] = 0;

            var sentMatchRequest = net.sentMatchRequest;
            if (sentMatchRequest != null)
            {
                if (RayGui.GuiButton(stack.nextRectangle(150, 50), "Cancel"))
                {
                    net.CancelMatchRequest();
                }

                if (sentMatchRequest.received)
                {
                    RayGui.GuiLabel(stack.nextRectangle(150, 50), "Player has received request");
                } else
                {
                    RayGui.GuiLabel(stack.nextRectangle(150, 50), "Player has not yet received request");
                }

            } else
            {
                if (RayGui.GuiButton(stack.nextRectangle(150, 50), "Connect"))
                {
                    UInt16 port = 0;
                    bool parsePort = UInt16.TryParse(ConvertToStringFrombytes(connectPort), out port);

                    IPAddress? ipAddress = null;
                    bool parseIp = IPAddress.TryParse(ConvertToStringFrombytes(connectIp), out ipAddress);

                    if (parsePort && parseIp && ipAddress != null)
                    {
                        net.SendMatchRequest(new IPEndPoint(ipAddress, port));
                    }
                }
            }
        }

        Raylib.EndDrawing();
    }

    public static void ShowBoardScreen(float dt)
    {
        Vector2 WindowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.WHITE);

        Raylib.DrawFPS(12, 12);
        Raylib.HideCursor();

        Vector2 min = new Vector2(0, 0);
        Vector2 max = WindowSize;
        float targetWidth = 11;
        float targetHeight = 8;
        float avaliableWidth = (max - min).X;
        float avaliableHeight = (max - min).Y;
        float size = Math.Min(avaliableWidth / targetWidth, avaliableHeight / targetHeight);
        Vector2 offset = new Vector2((max.X - (int)(size * 11)) / 2, (max.Y - (int)(size * 8)) / 2);

        Vector2 position = Raylib.GetMousePosition();

        Vector2 sector = ((position - offset) / size) - new Vector2(size / 20, size / 20) / 100;
        myCursor.pos = new Vector2(sector.X / 11, sector.Y / 8);
        myCursor.type = Raylib.IsWindowFocused() ? CursorType.Default : CursorType.Hidden;

        DrawBoard(new Vector2(0, 0), WindowSize, offset);

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            Console.WriteLine(sector);
        }

        if (myCursor.type != CursorType.Hidden)
        {
            Raylib.DrawCircleV(new Vector2(myCursor.pos.X * 11, myCursor.pos.Y * 8) * size + offset, 10, Raylib.BLACK);
        }
        if (enemyCursor.type != CursorType.Hidden)
        {
            Raylib.DrawCircleV(new Vector2(enemyCursor.pos.X * 11, enemyCursor.pos.Y * 8) * size + offset, 10, Raylib.RED);
        }

        Raylib.EndDrawing();
    }

    public static void DrawBoard(Vector2 min, Vector2 max, Vector2 offset)
    {
        RlGl.rlPushMatrix();
        float targetWidth = 11;
        float targetHeight = 8;

        float avaliableWidth = (max - min).X;
        float avaliableHeight = (max - min).Y;

        float size = Math.Min(avaliableWidth / targetWidth, avaliableHeight / targetHeight);

        RlGl.rlTranslatef(offset.X, offset.Y, 0);


        Raylib.DrawRectangle((int)0, (int)0, (int)(size * 11), (int)(size * 8 + size / 8), Raylib.LIGHTGRAY);
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (((i % 2) ^ (j % 2)) == 0)
                    Raylib.DrawRectangle((int)(size*i) + (int)size/10, (int)(size * j)  + (int)size/10, (int)(size * 0.9), (int)(size * 0.9), Raylib.WHITE);
                else
                    Raylib.DrawRectangle((int)(size * i) + (int)size / 10, (int)(size * j) + (int)size / 10, (int)(size * 0.9), (int)(size * 0.9), Raylib.BLACK);
            }
        }
        Raylib.DrawRectangle((int)size * 10, (int)(0 + 10), (int)(size), (int)(size / 2), Raylib.GRAY);
        float center = (float)(size / 2 * 0.9 +(int)size / 10);
        foreach (Chesspiece piece in chesspieces)
        {
            if (piece.type == PieceType.Pawn)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Raylib.DARKGRAY);
            }
            if (piece.type == PieceType.Rook)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Raylib.DARKPURPLE);
            }
            if (piece.type == PieceType.Bishop)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Raylib.DARKBLUE);
            }
            if (piece.type == PieceType.Knight)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Raylib.DARKGREEN);
            }
            if (piece.type == PieceType.King)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Raylib.RED);
            }
            if (piece.type == PieceType.Queen)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Raylib.DARKBROWN);
            }
        }
        RlGl.rlPopMatrix();
    }
}
