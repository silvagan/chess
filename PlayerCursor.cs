﻿using Raylib_CsLo;
using System.Numerics;

namespace chess
{
    internal enum CursorType
    {
        Default,
        Hidden,
        Grab
    }

    internal class PlayerCursor
    {
        public Vector2 pos {get; set; }
        public int chessPiece { get; set; }
        public CursorType type { get; set; }

        public PlayerCursor(Vector2 pos, int chessPiece, CursorType type)
        {
            this.pos = pos;
            this.chessPiece = chessPiece;
            this.type = type;
        }
    }
}
