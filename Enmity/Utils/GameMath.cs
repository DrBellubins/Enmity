using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using Raylib_cs;

namespace Enmity.Utils
{
    public static class GameMath
    {
        private static uint xorRND;
        public static uint xorSeed;

        private const double xorMaxRatio = 1.0 / uint.MaxValue;

        public static void InitXorRNG()
        {
            xorRND = (uint)System.DateTime.UtcNow.Ticks;
        }

        public static float GetXorFloat(float n = 1f)
        {
            xorRND ^= xorRND << 21;
            xorRND ^= xorRND >> 35;
            xorRND ^= xorRND << 4;
            return (float)(xorRND * xorMaxRatio * n);
        }

        public static float GetXorFloat(float min, float max)
        {
            xorRND ^= xorRND << 21;
            xorRND ^= xorRND >> 35;
            xorRND ^= xorRND << 4;
            return min + (float)(xorRND * xorMaxRatio * (max - min));
        }

        public static Color RandomRGB(int alpha = 255)
        {
            return new Color((int)(GetXorFloat() * 255f), (int)(GetXorFloat() * 255f),
                (int)(GetXorFloat() * 255f), alpha);
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                value = min;
            else
            {
                if (value > max)
                    value = max;
            }

            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else
            {
                if (value > max)
                    value = max;
            }

            return value;
        }

        public static float Repeat(float value, float min, float max)
        {
            if (value < min)
                value = max;
            else
            {
                if (value > max)
                    value = min;
            }

            return value;
        }

        public static int Repeat(int value, int min, int max)
        {
            if (value < min)
                value = max;
            else
            {
                if (value > max)
                    value = min;
            }

            return value;
        }

        public static float Smoothstep(float edge0, float edge1, float x)
        {
            // Scale, bias and saturate x to 0..1 range
            x = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);

            // Evaluate polynomial
            return x * x * (3 - 2 * x);
        }

        public static float Lerp(float a, float b, float t)
        {
            return (1f - t) * a + t * b;
        }

        public static Vector2 GetNearestChunkCoord(Vector2 input)
        {
            int x = (int)MathF.Floor(input.X);
            int y = (int)MathF.Floor(input.Y);

            int xRem = x % 32;
            int yRem = y % 32;

            return new Vector2(x - xRem, y - yRem);
        }

        public static Vector2 GetNearestBlockCoord(Vector2 input)
        {
            return new Vector2(MathF.Round(input.X) - 0.5f, MathF.Round(input.Y) - 0.5f);
        }

        public static bool Vector2Equals(Vector2 a, Vector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
    }
}
