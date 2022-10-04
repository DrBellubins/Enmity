using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

using Enmity.Utils;

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
            Camera.target = Position;
            Camera.offset = new Vector2(UI.CenterPivot.X, UI.CenterPivot.Y);
            Camera.rotation = 0.0f; // Flip camera so that north is +Y
            Camera.zoom = 100.0f;
        }

        public void Update(float deltaTime)
        {
            if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
            {
                Position.Y -= 10f * deltaTime;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
            {
                Position.Y += 10f * deltaTime;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
            {
                Position.X -= 10f * deltaTime;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
            {
                Position.X += 10f * deltaTime;
            }

            Camera.zoom += Raylib.GetMouseWheelMove();
            Camera.target = Position;
        }

        public void Draw()
        {
            Raylib.DrawCircleV(Position, 0.45f, Color.BLUE);
        }
    }
}
