using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace UnveiledMystery
{
    public class CameraManager : ModPlayer
    {
        public int magnitudeMax;
        public bool isShaking;
        public float shakeTimer = 0;
        public float shakeTimerMax = 0;


        public override void ModifyScreenPosition()
        {

            if (isShaking)
            {
                shakeTimer++;
                int magnitude = (int)MathHelper.Lerp(magnitudeMax, 0, shakeTimer / shakeTimerMax);
                Main.screenPosition += new Vector2(Main.rand.Next(-magnitude, magnitude), Main.rand.Next(-magnitude, magnitude));
                if (shakeTimer >= shakeTimerMax)
                {
                    isShaking = false;
                    shakeTimer = 0;
                }
            }
        }

        public void Shake(int magn, float TimerMax)
        {
            magnitudeMax = magn;
            shakeTimerMax = TimerMax;
            isShaking = true;
        }



    }
}
