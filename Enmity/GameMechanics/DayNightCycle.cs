using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

using Enmity.Utils;
using static Enmity.Utils.GameMath;

namespace Enmity.GameMechanics
{
    // TODO: Time sometimes skips (fixed??)
    // TODO: Sky transitions too quickly
    // TODO: Sky transitions are offset from TimeOfDay
    internal class DayNightCycle
    {
        public const float TimeMultiplier = 100f; // 24 hour cycle happens in 14.4 mins

        public float TimeOfDay = 0f; // 0 to 24
        public float SkyBrightness = 0f; // 0 to 1

        private Rectangle skyRect = new Rectangle();

        private Color currentSkyColor = new Color();

        private Color daySkyColor = new Color(100, 190, 255, 255);
        private Color nightSkyColor = new Color(0, 20, 50, 255);

        public void Initialize()
        {
            TimeOfDay = 6f;

            skyRect.width = Engine.ScreenWidth;
            skyRect.height = Engine.ScreenHeight;
        }

        public void Update(float delaTime)
        {
            TimeOfDay += delaTime;
            TimeOfDay %= 24f;
        }

        public void Draw(float delaTime)
        {
            if (TimeOfDay > 0f && TimeOfDay <= 12f)
                SkyBrightness += delaTime / 12f;
            else if (TimeOfDay > 12f && TimeOfDay <= 24f)
                SkyBrightness -= delaTime / 12f;

            //SkyBrightness %= 1f; // Breaks

            SkyBrightness = Clamp(SkyBrightness, 0f, 1f);

            currentSkyColor = ColorLerp(nightSkyColor, daySkyColor, SkyBrightness);

            Raylib.DrawRectangleGradientV(0, 0, Engine.ScreenWidth, Engine.ScreenHeight, ColorRGBMultVal(currentSkyColor, 0.5f), currentSkyColor);

            Debug.DrawText($"Time of day: {TimeOfDay}");
            Debug.DrawText($"Sky brightness: {SkyBrightness}");
        }
    }
}
