using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

using Raylib_cs;
using Enmity.Utils;

namespace Enmity
{
    internal class Engine
    {
        public const int FPS = 60;
        public const float FrameTimestep = 1.0f / (float)FPS;

        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;

        public static bool IsRunning;
        public static bool IsPaused;

        public static Font MainFont;

        public void Initialize()
        {
            Raylib.InitWindow(1920, 1080, "Enmity dev");
            Raylib.SetExitKey(KeyboardKey.KEY_Q);
            Raylib.SetTargetFPS(60);

            MainFont = Raylib.LoadFontEx("Assets/Font/VarelaRound-Regular.ttf", 64, null, 250);

            var previousTimer = DateTime.Now;
            var currentTimer = DateTime.Now;

            var time = 0.0f;
            var deltaTime = 0.0f;

            IsRunning = true;
            IsPaused = false;

            // Initialize
            GameMath.InitXorRNG();
            Terrain.Block.InitializeBlockPrefabs();

            Debug.Initialize();

            var terrain = new Terrain.TerrainGeneration();
            terrain.Initialize(new Vector2(0f, 128f));

            var player = new Player();
            player.Initialize();

            while (IsRunning)
            {
                if (Raylib.WindowShouldClose())
                    Close();

                currentTimer = DateTime.Now;

                if (IsPaused)
                    deltaTime = 0.0f;
                else
                    //deltaTime = FrameTimestep;
                    deltaTime = (currentTimer.Ticks - previousTimer.Ticks) / 10000000f;
                time += deltaTime;

                // Update
                Debug.Update();
                terrain.Update(player.Position);
                player.Update(deltaTime, terrain.ColliderCheckArray);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);

                Raylib.BeginMode2D(player.Camera);

                // World draw
                terrain.Draw(player.Position);
                player.Draw(deltaTime);

                Raylib.EndMode2D();

                // UI Draw
                Debug.Draw(time, deltaTime, player.Position);

                Raylib.EndDrawing();

                previousTimer = currentTimer;
            }

            Raylib.CloseWindow();
        }

        public static void Close()
        {
            IsRunning = false;
        }
    }
}
