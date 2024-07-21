using Raylib_cs;
using System.Drawing;
using System.Net;
using System.Numerics;
using Color = Raylib_cs.Color;

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
        net.enemyEndpoint = new IPEndPoint(IPAddress.Parse("192.168.0.106"), 8080);

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

            Vector2 min = new Vector2(0, 0);
            Vector2 max = WindowSize;
            float targetWidth = 11;
            float targetHeight = 8;
            float avaliableWidth = (max - min).X;
            float avaliableHeight = (max - min).Y;
            float size = Math.Min(avaliableWidth / targetWidth, avaliableHeight / targetHeight);
            Vector2 offset = new Vector2((max.X - (int)(size * 11)) / 2, (max.Y - (int)(size * 8)) / 2);

            DrawBoard(new Vector2(0, 0), WindowSize, offset);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 position = Raylib.GetMousePosition();
                Vector2 sector = ((position - offset)/size) - new Vector2(size / 20, size / 20)/100;
                Console.WriteLine(sector);

            }

            Raylib.DrawCircleV(myCursor.pos, 10, Color.Black);
            Raylib.DrawCircleV(enemyCursor.pos, 10, Color.Red);


            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    public static void DrawBoard(Vector2 min, Vector2 max, Vector2 offset)
    {
        Rlgl.PushMatrix();
        float targetWidth = 11;
        float targetHeight = 8;

        float avaliableWidth = (max - min).X;
        float avaliableHeight = (max - min).Y;

        float size = Math.Min(avaliableWidth / targetWidth, avaliableHeight / targetHeight);

        Rlgl.Translatef(offset.X, offset.Y, 0);


        Raylib.DrawRectangle((int)0, (int)0, (int)(size * 11), (int)(size * 8 + size / 8), Color.LightGray);
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (((i % 2) ^ (j % 2)) == 0)
                    Raylib.DrawRectangle((int)(size*i) + (int)size/10, (int)(size * j)  + (int)size/10, (int)(size * 0.9), (int)(size * 0.9), Color.White);
                else
                    Raylib.DrawRectangle((int)(size * i) + (int)size / 10, (int)(size * j) + (int)size / 10, (int)(size * 0.9), (int)(size * 0.9), Color.Black);
            }
        }
        Raylib.DrawRectangle((int)size * 10, (int)(0 + 10), (int)(size), (int)(size / 2), Color.Gray);
        Raylib.DrawRectangle((int)size * 10, (int)(0 + 10), (int)(size), (int)(size / 2), Color.Gray);
        float center = (float)(size / 2 * 0.9 +(int)size / 10);
        foreach (Chesspiece piece in chesspieces)
        {
            if (piece.type == PieceType.Pawn)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Color.DarkGray);
            }
            if (piece.type == PieceType.Rook)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Color.DarkPurple);
            }
            if (piece.type == PieceType.Bishop)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Color.DarkBlue);
            }
            if (piece.type == PieceType.Knight)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Color.DarkGreen);
            }
            if (piece.type == PieceType.King)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Color.Red);
            }
            if (piece.type == PieceType.Queen)
            {
                Raylib.DrawCircle((int)(size * piece.pos.X)+ (int)center, (int)(size * piece.pos.Y)+ (int)center, size / 4, Color.DarkBrown);
            }
        }
        Rlgl.PopMatrix();
    }
}
