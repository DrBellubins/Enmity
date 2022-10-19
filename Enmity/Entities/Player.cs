﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

using Enmity.Utils;
using Enmity.Terrain;

// TODO: Implement fall damage (fix ground check first)

namespace Enmity.Entities
{
    internal class Player
    {
        // Movement
        public const float WalkSpeed = 0.5f;
        public const float RunSpeed = 1.45f;

        // Mechanics
        public int Health = 100;

        // Entity
        public Vector2 Position;
        public float Rotation;

        public Camera2D Camera;

        // Various
        private bool grounded;
        private bool wasGrounded;
        private bool wasFalling;
        private float fallStartY;

        private bool canJump;
        private bool canRun;
        private float currentSpeed;

        private Vector2 acceleration;
        private Vector2 velocity;
        private Vector2 lastPosition;

        private bool isFalling { get { return grounded && velocity.Y < 0f; } }

        public void Initialize()
        {
            Position = new Vector2(0f, 32f);

            Camera = new Camera2D();
            Camera.target = Position;
            Camera.offset = new Vector2(UI.CenterPivot.X, UI.CenterPivot.Y);
            Camera.rotation = 0f; // Flip camera so that north is +Y
            Camera.zoom = 100f;
        }

        public void Update(float deltaTime, Block[,] collCheck)
        {
            lastPosition = Position;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && canRun)
                currentSpeed = RunSpeed;
            else
                currentSpeed = WalkSpeed;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
                acceleration.X -= currentSpeed;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
                acceleration.X += currentSpeed;

            Position.X += velocity.X;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    if (collCheck[x, y] != null)
                    {
                        var checkPos = collCheck[x, y].Position;

                        var isCollidingX = Raylib.CheckCollisionCircleRec(Position,
                            0.45f, new Rectangle(checkPos.X, checkPos.Y, 1.0f, 1.0f));

                        var distance = Vector2.Distance(Position, checkPos + new Vector2(0.5f, 0.5f));

                        if (isCollidingX)
                        {
                            if (collCheck[x, y].IsWall)
                                Position.X = lastPosition.X;
                        }
                    }
                }
            }

            Position.Y += velocity.Y;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    if (collCheck[x, y] != null)
                    {
                        var checkPos = collCheck[x, y].Position;

                        var isCollidingY = Raylib.CheckCollisionCircleRec(Position,
                            0.45f, new Rectangle(checkPos.X, checkPos.Y, 1.0f, 1.0f));

                        var distance = Vector2.Distance(Position, checkPos + new Vector2(0.5f, 0.5f));

                        if (isCollidingY)
                        {
                            if (collCheck[x, y].IsWall)
                                Position.Y = lastPosition.Y;
                        }
                    }
                }
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE))
                acceleration.Y = -0.7f;
            else
                acceleration.Y = 1.3f;

            velocity = Vector2.Lerp(velocity, Vector2.Zero, 0.2f);
            velocity += acceleration * deltaTime;

            // Reset velocity/acceleration
            acceleration = Vector2.Zero;
            //acceleration = new Vector2(0f, 0.04f);
            //velocity = Vector2.Lerp(velocity, Vector2.Zero, 17f * deltaTime);

            Camera.zoom = GameMath.Clamp(Camera.zoom + Raylib.GetMouseWheelMove(), 15f, 100f);
            Camera.target = Vector2.Lerp(Camera.target, Position, 3.5f * deltaTime);
        }

        public void Draw(float deltaTime)
        {
            Debug.DrawText($"grounded: {grounded}");
            Raylib.DrawCircleV(Position, 0.45f, Color.GREEN);
        }
    }
}
