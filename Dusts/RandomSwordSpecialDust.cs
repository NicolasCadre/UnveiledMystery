using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace UnveiledMystery.Dusts
{
    public class RandomSwordSpecialDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.frame = new Rectangle(0, 0, 20, 20);
            dust.scale = 0.2f;
            dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.99f;
            dust.scale *= 0.99f;


            if (dust.scale < 0.01f)
            {
                dust.active = false;
            }

            return false;
        }
    }
}