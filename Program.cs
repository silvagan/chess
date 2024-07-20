using Raylib_cs;
using System.Net;
using System.Numerics;

namespace chess;

class Program
{
    public static List<Chesspiece> chesspieces = new List<Chesspiece>();
    public static void Main()
    {
        int i = 0;

        PlayerCursor myCursor = new PlayerCursor(Vector2.Zero, 0, CursorType.Default);
        PlayerCursor enemyCursor = new PlayerCursor(Vector2.Zero, 0, CursorType.Default);

        var net = new ChessClient(myCursor, enemyCursor);

        Console.WriteLine($"Listening on port: {net.getPort()}");
        net.enemyEndpoint = new IPEndPoint(IPAddress.Parse("192.168.0.103"), 8080);

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

        Raylib.SetTargetFPS(144);
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(1000, 1000, "Chess innit");

        while (!Raylib.WindowShouldClose())
        {
            myCursor.pos = Raylib.GetMousePosition();
            net.Update();

            Vector2 WindowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);

            Raylib.DrawFPS(12, 12);
            Raylib.HideCursor();
            
            DrawBoard(new Vector2(0, 0), WindowSize);

            Raylib.DrawCircleV(myCursor.pos, 10, Color.Black);
            Raylib.DrawCircleV(enemyCursor.pos, 10, Color.Red);


            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    public static void DrawBoard(Vector2 min, Vector2 max)
    {
        float tileSize = (int)Math.Min(max.X - min.X, max.Y - min.Y) / 8;
        Vector2 offset = new Vector2((max.X - (int)(tileSize * 8 + tileSize / 8))/2 + tileSize/10, (max.Y - (int)(tileSize * 8 + tileSize/10))/2+ tileSize / 8);
        Raylib.DrawRectangle((int)0, (int)offset.Y-10, (int)(tileSize * 11), (int)(tileSize * 8 + tileSize / 8), Color.LightGray);
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (((i % 2) ^ (j % 2)) == 0)
                    Raylib.DrawRectangle((int)(tileSize*i) + 10, (int)(tileSize * j + offset.Y), (int)(tileSize * 0.9), (int)(tileSize * 0.9), Color.White);
                else
                    Raylib.DrawRectangle((int)(tileSize * i) + 10, (int)(tileSize * j + offset.Y), (int)(tileSize * 0.9), (int)(tileSize * 0.9), Color.Black);
            }
        }

        foreach(Chesspiece piece in chesspieces)
        {
            if (piece.type == PieceType.Pawn)
            {
                Raylib.DrawCircle((int)(tileSize * (piece.pos.X + 0.5)) + 4, (int)(tileSize * (piece.pos.Y + 0.5) + offset.Y) - 4, tileSize / 4, Color.DarkGray);
            }
            if (piece.type == PieceType.Rook)
            {
                Raylib.DrawCircle((int)(tileSize * (piece.pos.X + 0.5)) + 4, (int)(tileSize * (piece.pos.Y + 0.5) + offset.Y) - 4, tileSize / 4, Color.DarkPurple);
            }
            if (piece.type == PieceType.Bishop)
            {
                Raylib.DrawCircle((int)(tileSize * (piece.pos.X + 0.5)) + 4, (int)(tileSize * (piece.pos.Y + 0.5) + offset.Y) - 4, tileSize / 4, Color.DarkBlue);
            }
            if (piece.type == PieceType.Knight)
            {
                Raylib.DrawCircle((int)(tileSize * (piece.pos.X + 0.5)) + 4, (int)(tileSize * (piece.pos.Y + 0.5) + offset.Y) - 4, tileSize / 4, Color.DarkGreen);
            }
            if (piece.type == PieceType.King)
            {
                Raylib.DrawCircle((int)(tileSize * (piece.pos.X + 0.5)) + 4, (int)(tileSize * (piece.pos.Y + 0.5) + offset.Y) - 4, tileSize / 4, Color.Red);
            }
            if (piece.type == PieceType.Queen)
            {
                Raylib.DrawCircle((int)(tileSize * (piece.pos.X + 0.5)) + 4, (int)(tileSize * (piece.pos.Y + 0.5) + offset.Y) - 4, tileSize / 4, Color.DarkBrown);
            }
        }
    }
}
