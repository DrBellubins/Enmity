using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

namespace Enmity.GameEngine
{
    // Circles aren't actually objects in raylib,
    // so this is just a shell class iterated later.
    internal class CircleCollider : Collider
    {
        public float Radius;

        public CircleCollider() : base()
        {
            Radius = 1f;
        }

        public CircleCollider(float radius) : base()
        {
            Radius = radius;
        }

        // Boiler plate wrappers for quality-of-life
        public bool CheckCollisionCircle(Vector2 checkPos, float checkRadius)
        {
            return Raylib.CheckCollisionCircles(Position, Radius, checkPos, checkRadius);
        }

        public bool CheckCollisionRec(Rectangle checkRec)
        {
            return Raylib.CheckCollisionCircleRec(Position, Radius, checkRec);
        }

        public bool CheckCollisionPoint(Vector2 checkPoint)
        {
            return Raylib.CheckCollisionPointCircle(checkPoint, Position, Radius);
        }
    }
}
