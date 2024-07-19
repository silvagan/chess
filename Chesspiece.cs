using Raylib_cs;
using System.Numerics;

namespace chess
{
    internal enum PieceType
    {
        Pawn, Bishop, Knight, Rook, Queen, King
    }

    internal class Chesspiece
    {
        public int key { get; set; }
        public Vector2 pos { get; set; }
        public PieceType type { get; set; }

        public Chesspiece(int key, Vector2 pos, PieceType type)
        {
            this.key = key;
            this.pos = pos;
            this.type = type;
        }
    }
}
