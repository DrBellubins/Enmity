using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Enmity.GameEngine
{
    internal abstract class Collider
    {
        public static List<Collider> ColliderPool = new List<Collider>();

        public Vector2 Position;

        public Collider()
        {
            ColliderPool.Add(this);
        }

        ~Collider()
        {
            ColliderPool.Remove(this);
        }
    }
}
