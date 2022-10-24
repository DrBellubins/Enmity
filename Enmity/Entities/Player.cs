using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

using Enmity.Utils;
using Enmity.Terrain;
using Enmity.GameEngine;

using static Enmity.Utils.GameMath;

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

        public bool Falling { get { return !grounded && velocity.Y < 0f; } }

        // Various
        private bool grounded;
        private bool wasGrounded;
        private bool wasFalling;
        private float fallStartY;

        private bool canJump;
        private bool canRun;
        private float currentSpeed;

        private Vector2 spawnPosition;

        // Movement
        private Vector2 acceleration;
        private Vector2 velocity;
        private Vector2 lastPosition;

        private Sound painSound;

        public void Initialize(Vector2 spawnPos)
        {
            painSound = Raylib.LoadSound("Assets/Sounds/Player/pain.ogg");

            spawnPosition = spawnPos;
            Respawn();

            Camera = new Camera2D();
            Camera.target = Position;
            Camera.offset = new Vector2(UI.CenterPivot.X, UI.CenterPivot.Y);
            Camera.rotation = 0f; // Flip camera so that north is +Y
            Camera.zoom = 100f;
        }

        public void Update(float deltaTime, Block[,] collCheck)
        {
            grounded = false; // Needs to be reset cause raycast is finicky
            lastPosition = Position;

            // Movement
            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && canRun)
                currentSpeed = RunSpeed;
            else
                currentSpeed = WalkSpeed;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
                acceleration.X -= currentSpeed;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
                acceleration.X += currentSpeed;

            // Hover & gravity
            if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE))
                acceleration.Y = -0.7f;
            else
                acceleration.Y = 2.7f;

            // Collision checks (x & y)
            Position.X += velocity.X;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    if (collCheck[x, y] != null)
                    {
                        var collider = new SquareCollider(1.0f, 1.0f);
                        collider.Position = collCheck[x, y].Position;

                        var isCollidingX = collider.CheckCollisionCircle(this.Position, 0.45f);

                        if (isCollidingX)
                        {
                            if (collCheck[x, y].IsWall)
                            {
                                Position.X = lastPosition.X;
                                velocity.X = 0f;
                            }
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
                        var collider = new SquareCollider(1.0f, 1.0f);
                        collider.Position = collCheck[x, y].Position;

                        var isCollidingY = collider.CheckCollisionCircle(this.Position, 0.45f);

                        if (isCollidingY)
                        {
                            if (collCheck[x, y].IsWall)
                            {
                                Position.Y = lastPosition.Y;
                                velocity.Y = 0f;
                            }
                        }
                    }
                }
            }

            // Grounded check
            var blockBelow1 = collCheck[1, 3];
            var hit = new RaycastHit();

            if (blockBelow1 != null)
            {
                if (blockBelow1.IsWall)
                    grounded = Physics.Raycast(this.Position + new Vector2(0f, 0.451f), new Vector2(0f, 1f), 0.2f);
            }

            // Fall damage
            // TODO: Doesn't always trigger
            if (!wasFalling && Falling)
            {
                fallStartY = Position.Y;
                Console.WriteLine("Started falling");
            }

            if (!wasGrounded && grounded)
            {
                var fallDistance = MathF.Abs(fallStartY - Position.Y);

                if (fallDistance > 2f)
                    Damage((int)fallDistance);

                Console.WriteLine($"Fell {fallDistance} units");
            }

            Debug.DrawText($"Health: {Health}");
            Debug.DrawText($"grounded: {grounded}");
            Debug.DrawText($"falling: {Falling}");

            velocity = Vector2.Lerp(velocity, Vector2.Zero, 0.2f);
            velocity += acceleration * deltaTime;

            // Reset velocity/acceleration
            acceleration = Vector2.Zero;
            //acceleration = new Vector2(0f, 0.04f);
            //velocity = Vector2.Lerp(velocity, Vector2.Zero, 17f * deltaTime);

            // Death
            if (Health <= 0)
                Respawn();

            Camera.zoom = Clamp(Camera.zoom + Raylib.GetMouseWheelMove(), 15f, 100f);
            Camera.target = Vector2.Lerp(Camera.target, Position, 3.5f * deltaTime);

            wasGrounded = grounded;
            wasFalling = Falling;
        }

        public void Draw(float deltaTime)
        {
            Raylib.DrawCircleV(Position, 0.45f, Color.GREEN);
        }

        public void Damage(int damage)
        {
            Health = Clamp(Health - damage, 0, 100);
            Raylib.PlaySound(painSound);
        }

        public void Respawn()
        {
            Position = spawnPosition;
            //Position = new Vector2(0f, 32f);
            Health = 100;
        }
    }
}
