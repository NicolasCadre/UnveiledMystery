using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using UnveiledMystery.Buffs;
using System;
namespace UnveiledMystery.Enemies
{
    public class LivingTrapMinion_Projectile : ModProjectile
    {
        bool canJump = false;
        bool shouldJumpHigher = false;
        private float timerBeforeJump
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float ensuringProjectileTouchFloorTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Minion Boss Pet");

            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            //Projectile.CloneDefaults(ProjectileID.CompanionCube); 

            Projectile.width = 32;
            Projectile.height = 32;
            DrawOriginOffsetY = -20;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = true;
        }


        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Vector2 vector = player.Center - Projectile.Center;
            float distanceLenght = vector.Length();
            if (distanceLenght >= 200)
            {
                timerBeforeJump++;
                if (timerBeforeJump >= 60 && canJump)
                    Jump(vector, player);

                    

            }
            if (Main.myPlayer == player.whoAmI && distanceLenght >= 2000)
            {
                Projectile.position = player.Center;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }

            Projectile.velocity.Y += 0.1f;
            Projectile.rotation += Projectile.velocity.X / 20f;

            float overlapVelocity = 0.04f;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];

                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X)
                    {
                        Projectile.velocity.X -= overlapVelocity;
                    }
                    else
                    {
                        Projectile.velocity.X += overlapVelocity;
                    }

                    if (Projectile.position.Y < other.position.Y)
                    {
                        Projectile.velocity.Y -= overlapVelocity;
                    }
                    else
                    {
                        Projectile.velocity.Y += overlapVelocity;
                    }
                }
            }



            CheckActive(player);
        }


        private void Jump(Vector2 vector, Player player)
        {
            float speed = vector.Length()/100;
            vector.Normalize();
            vector *= speed;
            Vector2 velocity;
            Point tilePosition = (Projectile.Center + new Vector2(0, Projectile.height / 2)).ToTileCoordinates();

            if(player.position.X < Projectile.position.X)
            {
                Projectile.spriteDirection = 1;
                if (WorldGen.SolidOrSlopedTile(tilePosition.X - 2, tilePosition.Y))
                {
                    if(shouldJumpHigher)
                    {
                        velocity = Projectile.velocity + new Vector2(+3, -5);
                        shouldJumpHigher = false;
                    }
                    else
                        velocity = Projectile.velocity + new Vector2(+3, -2);

                    Projectile.velocity = velocity;
                    shouldJumpHigher = true;

                }
                else
                {
                    if (shouldJumpHigher)
                    {
                        velocity = Projectile.velocity + (player.position.Y <= Projectile.position.Y ? vector : new Vector2(vector.X, 0)) + new Vector2(0, -5);
                        shouldJumpHigher = false;
                    }
                    else
                        velocity = Projectile.velocity + (player.position.Y <= Projectile.position.Y ? vector : new Vector2(vector.X, 0)) + new Vector2(0, -2);
                    Projectile.velocity = velocity;

                }
            }
            else if (player.position.X > Projectile.position.X)
            {
                Projectile.spriteDirection = -1;
                if (WorldGen.SolidOrSlopedTile(tilePosition.X + 2, tilePosition.Y))
                {
                    if (shouldJumpHigher)
                    {
                        velocity = Projectile.velocity + new Vector2(-3, -5);
                        shouldJumpHigher = false;
                    }
                    else
                        velocity = Projectile.velocity + new Vector2(-3, -2);

                    Projectile.velocity = velocity;
                    shouldJumpHigher = true;
                }
                else
                {
                    if (shouldJumpHigher)
                    {
                        velocity = Projectile.velocity + (player.position.Y <= Projectile.position.Y ? vector : new Vector2(vector.X, 0)) + new Vector2(0, -5);
                        shouldJumpHigher = false;
                    }
                    else
                        velocity = Projectile.velocity + (player.position.Y <= Projectile.position.Y ? vector : new Vector2(vector.X, 0)) + new Vector2(0, -2);
                    Projectile.velocity = velocity;

                }
            }


            canJump = false;
            ensuringProjectileTouchFloorTimer = 0;
            timerBeforeJump = 0;
        }
        private void CheckActive(Player player)
        {
            if (!player.dead && player.HasBuff(ModContent.BuffType<LivingTrapMinion_Buff>()))
            {
                Projectile.timeLeft = 2;
            }
        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Point tilePosition = (Projectile.Center + new Vector2(0, Projectile.height / 2)).ToTileCoordinates();
            for (int i = -1; i <= 2; i++)
            {

                if (WorldGen.SolidOrSlopedTile(tilePosition.X + i, tilePosition.Y+1))
                {
                    Projectile.velocity.Y = 0;
                    Projectile.velocity.X = 0;
                    canJump = true;

                }
            }
            return base.OnTileCollide(oldVelocity);
        }

    }
}
