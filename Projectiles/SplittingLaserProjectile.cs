using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.Enums;
using static Terraria.ModLoader.ModContent;
using UnveiledMystery.Enemies.Boss;
using static Humanizer.In;

namespace UnveiledMystery.Projectiles
{
    public class SplittingLaserProjectile : ModProjectile
    {
        private bool runOnce;
        Player owner;
        // Distance charge particle from the player center
        private const float LASER_DISTANCE_FROM_PLAYER = 10;
        private const float LASER_MAXDISTANCE = 200;

        private const float TIMER_MAX = 2f;

        private int howManyLaser = 0;

        private const int STEP = 10;

        private int laserNumber = 1;


        private Vector2 startPos;
        public float Distance
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public float Timer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texture = Request<Texture2D>("UnveiledMystery/Projectiles/SplittingLaserProjectile");

            DrawLaser(Main.spriteBatch, texture.Value, startPos,
                Projectile.velocity, 1, Projectile.damage, -1.57f, 1f, 1000f, Color.White, (int)LASER_DISTANCE_FROM_PLAYER);

            return false;
        }

        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 50)
        {
            float r = unit.ToRotation() + rotation;

            // Draws the laser 'body'
            for (float i = transDist; i <= Distance; i += step)
            {
                Color c = Color.White;
                var origin = start + i * unit;
                spriteBatch.Draw(texture, origin - Main.screenPosition,
                    new Rectangle(0, 26, 28, 26), i < transDist ? Color.Transparent : c, r,
                    new Vector2(28 * .5f, 26 * .5f), scale, 0, 0);
            }

            // Draws the laser 'tail'
            spriteBatch.Draw(texture, start + unit * (transDist - step) - Main.screenPosition,
                new Rectangle(0, 0, 28, 26), Color.White, r, new Vector2(28 * .5f, 26 * .5f), scale, 0, 0);

            // Draws the laser 'head'
            spriteBatch.Draw(texture, start + (Distance + step) * unit - Main.screenPosition,
                new Rectangle(0, 52, 28, 26), Color.White, r, new Vector2(28 * .5f, 26 * .5f), scale, 0, 0);
        }

        // Change the way of collision check of the Projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {

            Vector2 unit = Projectile.velocity;
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), startPos,
                startPos + unit * Distance, 22, ref point);
        }

        // Set custom immunity time on hitting an NPC
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[Projectile.owner] = 5;
        }

        // The AI of the Projectile
        public override void AI()
        {

            owner = Main.player[Projectile.owner];

            if (!runOnce )
            {
                startPos = owner.Center;
                SetLaserPosition(owner);
                for (int i = 0; i <= Distance; i += STEP)
                {
                    howManyLaser++;
                }

                runOnce = true;
            }

            Projectile.position = startPos + Projectile.velocity * LASER_DISTANCE_FROM_PLAYER;

            Projectile.hide = false;

            SpawnDusts(owner);
            CastLights();
            SoundEngine.PlaySound(SoundID.Item132 with { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew });


            Timer++;


            if (Timer >= TIMER_MAX)
            {
                SoundEngine.PlaySound(SoundID.Item91);
                var endPos = Projectile.Center + Projectile.velocity * (Distance - LASER_DISTANCE_FROM_PLAYER);
                var laserPos = Vector2.Lerp(startPos, endPos, (float)laserNumber/howManyLaser);
                Vector2 dir = Projectile.velocity.RotatedBy(MathHelper.ToRadians(laserNumber % 2 ==0 ? 90 : -90));

                Projectile.NewProjectile(Projectile.GetSource_FromAI(), laserPos.X, laserPos.Y, dir.X, dir.Y, ModContent.ProjectileType<Projectiles.SplittingLaserSingleProjectile>(), 15 , 0f, Projectile.owner);
                Timer = 0;
                laserNumber++;

                if (laserNumber > howManyLaser)
                    Projectile.Kill();
            }

        }

        private void SpawnDusts(Player owner)
        {
            Vector2 unit = Projectile.velocity * -1;
            Vector2 dustPos = startPos + Projectile.velocity * Distance;

            for (int i = 0; i < 2; ++i)
            {
                float num1 = Projectile.velocity.ToRotation() + (Main.rand.Next(2) == 1 ? -1.0f : 1.0f) * 1.57f;
                float num2 = (float)(Main.rand.NextDouble() * 0.8f + 1.0f);
                Vector2 dustVel = new Vector2((float)Math.Cos(num1) * num2, (float)Math.Sin(num1) * num2);
                Dust dust = Main.dust[Dust.NewDust(dustPos, 0, 0, DustID.Electric, dustVel.X, dustVel.Y)];
                dust.noGravity = true;
                dust.scale = 1.2f;
                /*dust = Dust.NewDustDirect(startPos, 0, 0, 31,
                    -unit.X * Distance, -unit.Y * Distance);
                dust.fadeIn = 0f;
                dust.noGravity = true;
                dust.scale = 0.88f;
                dust.color = Color.Cyan;*/
            }

            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Projectile.velocity.RotatedBy(1.57f) * ((float)Main.rand.NextDouble() - 0.5f) * Projectile.width;
                Dust dust = Main.dust[Dust.NewDust(dustPos + offset - Vector2.One * 4f, 8, 8, DustID.Smoke, 0.0f, 0.0f, 100, new Color(), 1.5f)];
                dust.velocity *= 0.5f;
                dust.velocity.Y = -Math.Abs(dust.velocity.Y);
                unit = dustPos - startPos;
                unit.Normalize();
                dust = Main.dust[Dust.NewDust(startPos + 55 * unit, 8, 8, DustID.Smoke, 0.0f, 0.0f, 100, new Color(), 1.5f)];
                dust.velocity = dust.velocity * 0.5f;
                dust.velocity.Y = -Math.Abs(dust.velocity.Y);
            }
        }


        //Sets the end of the laser position based on where it collides with something

        private void SetLaserPosition(Player owner)
        {
            for (Distance = LASER_DISTANCE_FROM_PLAYER; Distance <= LASER_MAXDISTANCE; Distance += 5f)
            {
                var start = startPos + Projectile.velocity * Distance;
                if (!Collision.CanHit(startPos, 1, 1, start, 1, 1))
                {
                    Distance -= 5f;
                    break;
                }
            }
        }



        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (Distance - LASER_DISTANCE_FROM_PLAYER), 26, DelegateMethods.CastLight);
        }

        public override bool ShouldUpdatePosition() => false;

    }
}

