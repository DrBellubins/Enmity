using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

using Enmity.Utils;
using Enmity.Terrain;

namespace Enmity
{
    internal class Player
    {
        // Movement
        public const float WalkSpeed = 1.45f;
        public const float RunSpeed = 3.5f;

        public Vector2 Position;
        public float Rotation;

        public Camera2D Camera;

        private bool canRun;
        private float currentSpeed;

        private Vector2 acceleration;
        private Vector2 velocity;
        private Vector2 lastPosition;

        public void Initialize()
        {
            Position = new Vector2(0f, 32f);

            Camera = new Camera2D();
            Camera.target = Position;
            Camera.offset = new Vector2(UI.CenterPivot.X, UI.CenterPivot.Y);
            Camera.rotation = 0f; // Flip camera so that north is +Y
            Camera.zoom = 10f;
        }

        public void Update(float deltaTime, Block[,] collCheck)
        {
            lastPosition = Position;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
            {
                acceleration.X -= 1.0f * deltaTime;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
            {
                acceleration.X += 1.0f * deltaTime;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && canRun)
                currentSpeed = RunSpeed;
            else
                currentSpeed = WalkSpeed;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE))
                acceleration.Y -= 0.15f;

            Position.X += velocity.X * currentSpeed;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    if (collCheck[x, y] != null)
                    {
                        var checkPos = collCheck[x, y].Position;

                        var isCollidingX = Raylib.CheckCollisionCircleRec(Position,
                            0.4f, new Rectangle(checkPos.X, checkPos.Y, 1.0f, 1.0f));

                        if (isCollidingX)
                        {
                            if (collCheck[x, y].IsWall)
                                Position.X = lastPosition.X;
                            else if (collCheck[x, y].Info.Type == BlockType.Water)
                            {
                                velocity.X *= 0.25f; // TODO: Needs to be frame independent
                                canRun = false;
                            }
                            else
                                canRun = true;
                        }
                    }
                }
            }

            Position.Y += velocity.Y * currentSpeed;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    if (collCheck[x, y] != null)
                    {
                        var checkPos = collCheck[x, y].Position;

                        var isCollidingY = Raylib.CheckCollisionCircleRec(Position,
                            0.4f, new Rectangle(checkPos.X, checkPos.Y, 1.0f, 1.0f));

                        if (isCollidingY)
                        {
                            if (collCheck[x, y].IsWall)
                                Position.Y = lastPosition.Y;
                            else if (collCheck[x, y].Info.Type == BlockType.Water)
                            {
                                velocity.Y *= 0.25f;
                                canRun = false;
                            }
                            else
                                canRun = true;
                        }
                    }
                }
            }

            velocity = Vector2.Lerp(velocity, Vector2.Zero, 0.2f);
            velocity += acceleration;

            // Reset velocity/acceleration
            acceleration = new Vector2(0f, 0.1f);
            //velocity = Vector2.Lerp(velocity, Vector2.Zero, 17f * deltaTime);

            Camera.zoom += Raylib.GetMouseWheelMove();
            Camera.target = Position;
        }

        public void Draw(float deltaTime)
        {
            Debug.DrawText($"delta: {deltaTime}");
            Raylib.DrawCircleV(Position, 0.45f, Color.BLUE);
        }
    }
}
