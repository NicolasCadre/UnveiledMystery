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

namespace UnveiledMystery.Projectiles
{
    public class LivingTrapLaser : ModProjectile
    {
        bool runOnce = false;
        NPC owner;

        // Maximum charge value
        private float maxCharge = Main.expertMode ? 150f : 210f;
        // Distance charge particle from the player center
        private const float LASER_MAXDISTANCE = 0f;

        private Vector2 startPos;
        public float Distance
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public float Charge
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public bool IsAtMaxCharge => Charge >= maxCharge;

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;

            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texture = Request<Texture2D>("UnveiledMystery/Projectiles/LivingTrapLaser");

            // Draw Laser when Charged
            if (IsAtMaxCharge)
            {
                DrawLaser(Main.spriteBatch, texture.Value, owner.Center - new Vector2(50, -145),
                    Projectile.velocity, 1, Projectile.damage, -1.57f, 1f, 1000f, Color.White, (int)LASER_MAXDISTANCE);
            }
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
            if (!IsAtMaxCharge) return false;

            Vector2 unit = Projectile.velocity;
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), owner.Center - new Vector2(50, -145),
                owner.Center - new Vector2(50, -150) + unit * Distance, 22, ref point);
        }

        // Set custom immunity time on hitting an NPC
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[Projectile.owner] = 5;
        }

        // The AI of the Projectile
        public override void AI()
        {
            if (runOnce == false)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (!Main.npc[i].active || Main.npc[i].type != ModContent.NPCType<LivingTrapBoss>())
                    {
                        continue;
                    }

                    owner = Main.npc[i];
                }
                runOnce = true;
            }
            if (owner.life <= 0)
                Projectile.Kill();
            startPos = owner.Center - new Vector2(50, -145);

            Projectile.position = startPos + Projectile.velocity * LASER_MAXDISTANCE;
            Projectile.timeLeft = 500;



            ChargeLaser(owner);

            if (Charge < maxCharge) return;

            // After Laser is charged

            Projectile.hide = false;
            SetLaserPosition(owner);
            SpawnDusts(owner);
            CastLights();
            SoundEngine.PlaySound(SoundID.Item132 with { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew });

            if (Charge > maxCharge + 50f)
                Projectile.Kill();
        }

        private void SpawnDusts(NPC owner)
        {
            Vector2 unit = Projectile.velocity * -1;
            Vector2 dustPos = startPos + Projectile.velocity * Distance;

            for (int i = 0; i < 2; ++i)
            {
                float num1 = Projectile.velocity.ToRotation() + (Main.rand.Next(2) == 1 ? -1.0f : 1.0f) * 1.57f;
                float num2 = (float)(Main.rand.NextDouble() * 0.8f + 1.0f);
                Vector2 dustVel = new Vector2((float)Math.Cos(num1) * num2, (float)Math.Sin(num1) * num2);
                Dust dust = Main.dust[Dust.NewDust(dustPos, 0, 0, 226, dustVel.X, dustVel.Y)];
                dust.noGravity = true;
                dust.scale = 1.2f;
                dust = Dust.NewDustDirect(startPos, 0, 0, 31,
                    -unit.X * Distance, -unit.Y * Distance);
                dust.fadeIn = 0f;
                dust.noGravity = true;
                dust.scale = 0.88f;
                dust.color = Color.Cyan;
            }

            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Projectile.velocity.RotatedBy(1.57f) * ((float)Main.rand.NextDouble() - 0.5f) * Projectile.width;
                Dust dust = Main.dust[Dust.NewDust(dustPos + offset - Vector2.One * 4f, 8, 8, 31, 0.0f, 0.0f, 100, new Color(), 1.5f)];
                dust.velocity *= 0.5f;
                dust.velocity.Y = -Math.Abs(dust.velocity.Y);
                unit = dustPos - startPos;
                unit.Normalize();
                dust = Main.dust[Dust.NewDust(startPos + 55 * unit, 8, 8, 31, 0.0f, 0.0f, 100, new Color(), 1.5f)];
                dust.velocity = dust.velocity * 0.5f;
                dust.velocity.Y = -Math.Abs(dust.velocity.Y);
            }
        }


        //Sets the end of the laser position based on where it collides with something

        private void SetLaserPosition(NPC owner)
        {
            for (Distance = LASER_MAXDISTANCE; Distance <= 2200f; Distance += 5f)
            {
                var start = startPos + Projectile.velocity * Distance;
                if (!Collision.CanHit(startPos, 1, 1, start, 1, 1))
                {
                    Distance -= 5f;
                    break;
                }
            }
        }

        private void ChargeLaser(NPC owner)
        {
            Vector2 pos = startPos;//- new Vector2(10, 10);
            Charge++;

            int chargeFact = (int)(Charge / 20f);
        }


        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (Distance - LASER_MAXDISTANCE), 26, DelegateMethods.CastLight);
        }

        public override bool ShouldUpdatePosition() => false;

    }
}

