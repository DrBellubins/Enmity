using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

namespace Enmity
{
    internal class Player
    {
        public Vector2 Position;
        public float Rotation;

        public Camera2D Camera;

        public void Initialize()
        {
            Camera = new Camera2D();
            Camera.target = new Vector2(Position.X, Position.Y);
            Camera.offset = new Vector2(UI.CenterPivot.X, UI.CenterPivot.Y);
            Camera.rotation = 0.0f; // Flip camera so that north is +Y
            Camera.zoom = 100.0f;
        }

        public void Update()
        {

        }

        public void Draw()
        {

        }
    }
}
