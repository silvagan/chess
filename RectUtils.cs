using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace chess;

internal static class RectUtils
{
    public static Vector2 GetCenteredPosition(Rectangle container, Vector2 size)
    {
        return new Vector2(
            container.x + (container.width - size.X)/2,
            container.y + (container.height - size.Y)/2
        );
    }
}
