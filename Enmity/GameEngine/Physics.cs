using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

namespace Enmity.GameEngine
{

    internal struct RaycastHit
    {
        // TODO: Implement normal
        public Vector2 Position;
        public Collider Collider;
    }

    internal class Physics
    {
        // TODO: Raycasting seems return true a lot, and false only when first hitting...
        public static bool Raycast(Vector2 origin, Vector2 direction, float distance = float.PositiveInfinity)
        {
            //var result = false;

            for (int i = 0; i < Collider.ColliderPool.Count; i++)
            {
                // TODO: implement raycast against circles
                if (Collider.ColliderPool[i].GetType() == typeof(CircleCollider))
                {
                    var collider = (CircleCollider)Collider.ColliderPool[i];

                    //if (!result) // Prevent getting switched back to false even when hitting something
                    //    result = Raylib.CheckCollisionCircleRec(collider.Position, collider.Radius, rayRect);
                }
                else if (Collider.ColliderPool[i].GetType() == typeof(SquareCollider))
                {
                    var collider = (SquareCollider)Collider.ColliderPool[i];
                    var colliderLines = getLinesFromRectangle(collider.Rect);

                    for (int ii = 0; ii < colliderLines.Count; ii++)
                    {
                        var start = colliderLines.Keys.ElementAt(ii);
                        var end = colliderLines.Values.ElementAt(ii);

                        unsafe
                        {
                            if (Raylib.CheckCollisionLines(origin, origin + (direction * distance), start, end, null))
                                return true;
                        }
                    }
                }
                else
                    return false; // Can't raycast against base colliders
            }

            return false;
        }

        public static bool Raycast(Vector2 origin, Vector2 direction, ref RaycastHit hit, float distance = float.PositiveInfinity)
        {
            var returnHit = new RaycastHit();

            for (int i = 0; i < Collider.ColliderPool.Count; i++)
            {
                // TODO: implement raycast against circles
                if (Collider.ColliderPool[i].GetType() == typeof(CircleCollider))
                {
                    var collider = (CircleCollider)Collider.ColliderPool[i];

                    //if (!result) // Prevent getting switched back to false even when hitting something
                    //    result = Raylib.CheckCollisionCircleRec(collider.Position, collider.Radius, rayRect);
                }
                else if (Collider.ColliderPool[i].GetType() == typeof(SquareCollider))
                {
                    var collider = (SquareCollider)Collider.ColliderPool[i];
                    var colliderLines = getLinesFromRectangle(collider.Rect);

                    for (int ii = 0; ii < colliderLines.Count; ii++)
                    {
                        var start = colliderLines.Keys.ElementAt(ii);
                        var end = colliderLines.Values.ElementAt(ii);

                        unsafe
                        {
                            var hitPos = new Vector2();

                            if (Raylib.CheckCollisionLines(origin, origin + (direction * distance), start, end, &hitPos))
                            {
                                returnHit.Position = hitPos;
                                returnHit.Collider = collider;
                                hit = returnHit;

                                return true;
                            }
                        }
                    }
                }
                else
                    return false; // Can't raycast against base colliders
            }

            return false;
        }

        private static Dictionary<Vector2, Vector2> getLinesFromRectangle(Rectangle rect)
        {
            var result = new Dictionary<Vector2, Vector2>();

            float x = rect.x;
            float y = rect.y;

            var topLeft = new Vector2(x, y);
            var topRight = new Vector2(x + rect.width, y);
            var bottomLeft = new Vector2(x, y + rect.height);
            var bottomRight = new Vector2(x + rect.width, y + rect.height);

            result.Add(topLeft, topRight);
            result.Add(topRight, bottomRight);
            result.Add(bottomRight, bottomLeft);
            result.Add(bottomLeft, topLeft);

            return result;
        }
    }
}
