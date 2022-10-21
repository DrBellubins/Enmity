using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

namespace Enmity.GameEngine
{
    internal class SquareCollider : Collider
    {
        public Rectangle Rect = new Rectangle(0f, 0f, 1f, 1f);

        public SquareCollider() : base() {}

        public SquareCollider(float width, float height)
        {
            Rect.x = Position.X;
            Rect.y = Position.Y;
            Rect.width = width;
            Rect.height = height;
        }

        // Boiler plate wrappers for quality-of-life
        public bool CheckCollisionCircle(Vector2 checkPos, float checkRadius)
        {
            Rect.x = Position.X;
            Rect.y = Position.Y;
            return Raylib.CheckCollisionCircleRec(checkPos, checkRadius, Rect);
        }

        public bool CheckCollisionRec(Rectangle checkRec)
        {
            Rect.x = Position.X;
            Rect.y = Position.Y;
            return Raylib.CheckCollisionRecs(Rect, checkRec);
        }

        public bool CheckCollisionPoint(Vector2 checkPoint)
        {
            Rect.x = Position.X;
            Rect.y = Position.Y;
            return Raylib.CheckCollisionPointRec(checkPoint, Rect);
        }
    }
}
