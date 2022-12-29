using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace UnveiledMystery.Projectiles
{
    public class AutomaticPlayerDartTrapHomingProjectile : ModProjectile
    {
        NPC NPCtarget
        {
            get => Main.npc[(int)Projectile.ai[0]];
            set => Projectile.ai[0] = value.whoAmI;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Turret Homing Projectile");
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 1000;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.velocity = (NPCtarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero) *10f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (!NPCtarget.active)
            {
                float sqrMaxDetectDistance = 1000 * 1000;

                foreach (NPC target in Main.npc)
                {
                    if (target.CanBeChasedBy())
                    {
                        float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);

                        if (sqrDistanceToTarget < sqrMaxDetectDistance)
                        {
                            sqrMaxDetectDistance = sqrDistanceToTarget;
                            NPCtarget = target;
                        }
                    }
                }
            }
        }


        /*public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }*/

    }
}
