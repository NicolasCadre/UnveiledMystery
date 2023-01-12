using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using static Terraria.ModLoader.ModContent;
using MonoMod.RuntimeDetour;

namespace UnveiledMystery.Projectiles
{
    public class LivingTrapProjectileFromLaserWarning : ModProjectile
    {
        private int timerBeforeShoot = 0;
        private const int TIMERBEFORESHOOTMAX = 60;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 100;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1000;
            Projectile.light = 0.5f;
            Projectile.alpha = 255;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 0;
        }
        public override void AI()
        {
            timerBeforeShoot++;
            if (timerBeforeShoot >= TIMERBEFORESHOOTMAX)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.position.Y+Projectile.height, 0, -15, ModContent.ProjectileType<LivingTrapProjectileFromLaser>(), 50, 5f);
                Projectile.Kill();
            }
            Projectile.alpha = (int)MathHelper.Lerp(255,0, (float)timerBeforeShoot/60f);
        }
    }
}
