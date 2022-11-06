using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;

namespace UnveiledMystery.Enemies.Boss
{
    [AutoloadBossHead]
    public class LivingTrapBoss : ModNPC
    {
        enum ProjectileType
        {
            BOUNCING = 1,
            TRIPLE = 2
        }

        private ProjectileType type;
        private int ai;
        private Vector2 BossArenaLocation;
        //Timer
        private int walkingTimer = 0;
        private int ProjectileAttackTimer;
        private bool hasChangedProjectileAttackTimer = false;
        private bool hasChangedLaserAttackTimer = false;
        private int LaserTimerStart;
        private int LaserTimer;
        private float LaserChargingFlickeringTimer = 0;
        private float LaserChargingFlickeringTimerMax = 50;

        public bool hasFinishedPhaseTransition = false;
        public bool hasStartedPhaseTransition = false;
        private bool hasLaunchedLaser = false;

        private Projectile Laser;
        public Player player;
        public bool Enraged = false;

        //Animation
        private int frame = 0;
        private double counting;
        public int frameGlowmaskEye = 0;
        public int frameGlowmaskMouth = 1;
        private bool doOnce = false;




        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            DisplayName.SetDefault("Living Trap");
            Main.npcFrameCount[NPC.type] = 3;
        }

        public override void SetDefaults()
        {

            NPC.width = 200;
            NPC.height = 432;

            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.npcSlots = 5f;

            NPC.lifeMax = 6000;
            NPC.damage = 120;
            NPC.defense = 20;
            NPC.knockBackResist = 0f;

            NPC.value = Item.buyPrice(gold: 10);

            NPC.lavaImmune = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            if (!Main.dedServ)
            {
                Music = MusicID.Boss2;
            }

        }
        /*public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ItemType<TMMCBossTreasureBag>())); //this requires you to set BossBag in SetDefaults accordingly        }
        }*/
        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.6f * bossLifeScale);
            NPC.damage = (int)(NPC.damage * 0.25f);
        }

        private void ResetProjectileTimer()
        {
            ProjectileAttackTimer = Main.expertMode ? Main.rand.Next(100, 200) : Main.rand.Next(150, 250);
            type = (ProjectileType)Main.rand.Next(1, 3);
            hasChangedProjectileAttackTimer = true;
            NPC.netUpdate = true;
        }

        private void ResetLaserTimer()
        {
            LaserTimerStart = Main.expertMode ? Main.rand.Next(100, 200) : Main.rand.Next(150, 250);
            hasChangedLaserAttackTimer = true;
            LaserChargingFlickeringTimerMax = Main.expertMode ? 40 : 50;
            LaserChargingFlickeringTimer = 0;
            frameGlowmaskMouth = 1;
            Laser = null;
            NPC.netUpdate = true;
        }
        private void LaserAI()
        {
            // Reload the Laser
            if ((double)NPC.ai[1] >= LaserTimerStart + (Main.expertMode ? 150f : 200f))
            {
                LaserTimer = 0;
                hasChangedLaserAttackTimer = false;
            }
            // Shoot the Laser
            else if ((double)NPC.ai[1] == LaserTimerStart)
            {

                Vector2 shootPos = NPC.Center - new Vector2(20, -145);
                Vector2 shootVel = new Vector2(-1, 10) - shootPos;//+ new Vector2(Main.rand.NextFloat(-accuracy, accuracy), Main.rand.NextFloat(-accuracy, accuracy)); 
                shootVel.Normalize();
                shootVel *= 3.5f;

                Laser = Main.projectile[Projectile.NewProjectile(new EntitySource_Misc(""), shootPos.X, shootPos.Y, shootVel.X, 0, ModContent.ProjectileType<Projectiles.BossLaser>(), NPC.damage / 2, 5f)];
                NPC.netUpdate = true;
            }
            // Charge the Laser
            if ((double)NPC.ai[1] >= LaserTimerStart)
            {
                LaserChargingFlickeringTimer++;
                //Blinking Mouth Sign
                if (LaserChargingFlickeringTimer >= LaserChargingFlickeringTimerMax)
                {
                    LaserChargingFlickeringTimerMax = MathHelper.Lerp(Main.expertMode ? 40f : 50f, 10f, Main.expertMode ? NPC.ai[1] / (LaserTimerStart + 50f) : NPC.ai[1] / (LaserTimerStart + 100f));
                    frameGlowmaskMouth = frameGlowmaskMouth == 0 ? 1 : 0;
                    if (frameGlowmaskMouth == 0)
                        SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Boss/LivingTrapBossLaser")
                        {
                            Volume = 0.5f,
                            Pitch = MathHelper.Lerp(-1f, 1f, Main.expertMode ? NPC.ai[1] / (LaserTimerStart + 150f) : NPC.ai[1] / (LaserTimerStart + 210f))
                        });
                    LaserChargingFlickeringTimer = 0;
                }
            }
        }
        private void ProjectileAI(Vector2 target)
        {
            int distance = (int)Vector2.Distance(target, NPC.Center);
            if ((double)NPC.ai[0] < ProjectileAttackTimer)
            {
                frameGlowmaskEye = 2;
                NPC.netUpdate = true;
            }
            // Visual Attack Sign
            else if ((double)NPC.ai[0] >= ProjectileAttackTimer && (double)NPC.ai[0] < ProjectileAttackTimer + 120)
            {
                SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Boss/LivingTrapBossProjectile")
                {
                    MaxInstances = 1,
                    SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                });

                frame = 0;
                if (type == ProjectileType.BOUNCING)
                {
                    frameGlowmaskEye = 0;
                    Dust.NewDust(NPC.Center - new Vector2(0, 140), 30, 30, 131);
                }
                else if (type == ProjectileType.TRIPLE)
                {
                    frameGlowmaskEye = 1;
                    Dust.NewDust(NPC.Center - new Vector2(0, 140), 30, 30, 130);
                }
                NPC.netUpdate = true;
            }

            // Shoot
            else if ((double)NPC.ai[0] >= ProjectileAttackTimer + 120)
            {
                ShootProjectile(target);
                hasChangedProjectileAttackTimer = false;
                NPC.netUpdate = true;
                ai = 0;
            }
        }

        private void ShootProjectile(Vector2 target)
        {
            Vector2 shootPos = NPC.Center - new Vector2(0, 140);
            Vector2 shootVel = target - shootPos;//+ new Vector2(Main.rand.NextFloat(-accuracy, accuracy), Main.rand.NextFloat(-accuracy, accuracy)); 
            shootVel.Normalize();
            shootVel *= 3.5f;
            switch (type)
            {
                //Triple (or Quintuple) laser
                case ProjectileType.TRIPLE:
                    {
                        float rotation = MathHelper.ToRadians(Main.expertMode ? 20 : 15);
                        int maxBullet = Main.expertMode ? 5 : 3;
                        for (int d = 0; d < 100; d++)
                        {
                            Vector2 RotatedVel = shootVel.RotatedBy(MathHelper.Lerp(-rotation, rotation, (float)d / ((float)100 - 1)));

                            Dust.NewDust(shootPos, 30, 30, 130, RotatedVel.X * 0.5f, RotatedVel.Y * 0.5f, 0, default(Color), 1f);
                        }
                        for (int i = 0; i < maxBullet; i++)
                        {

                            Vector2 RotatedVel = shootVel.RotatedBy(MathHelper.Lerp(-rotation, rotation, (float)i / ((float)maxBullet - 1)));

                            Projectile.NewProjectile(new EntitySource_Misc(""), shootPos.X, shootPos.Y, RotatedVel.X * 0.75f, RotatedVel.Y * 0.75f, ProjectileID.EyeLaser, NPC.damage / 3, 5f);
                        }
                        break;

                    }
                // Bouncing Laser
                case ProjectileType.BOUNCING:
                    {
                        float rotation = Main.expertMode ? MathHelper.ToRadians(Main.rand.NextFloat(0, 30)) : MathHelper.ToRadians(0);
                        int maxBullet = Main.expertMode ? 2 : 1;
                        SoundEngine.PlaySound(SoundID.Item12);

                        for (int d = 0; d < 100; d++)
                        {
                            Vector2 RotatedVel = shootVel.RotatedBy(MathHelper.Lerp(-rotation, rotation, (float)d / ((float)100 - 1)));
                            Dust.NewDust(shootPos, 30, 30, 131, RotatedVel.X * 2f, RotatedVel.Y * 2f, 0, default(Color), 1f);
                        }
                        for (int i = 0; i < maxBullet; i++)
                        {
                            Vector2 RotatedVel = shootVel.RotatedBy(MathHelper.Lerp(-rotation, rotation, (float)i / ((float)maxBullet - 1)));
                            Projectile.NewProjectile(new EntitySource_Misc(""), shootPos.X, shootPos.Y, Main.expertMode ? RotatedVel.X * 1.5f : shootVel.X * 1.5f, Main.expertMode ? RotatedVel.Y * 1.5f : shootVel.Y * 1.5f, ModContent.ProjectileType<Projectiles.BossBouncingBullet>(), NPC.damage / 3, 5f);

                        }
                    }
                    break;
            }
        }

        // Do what happen when boss is half hp
        private void PhaseTransition()
        {
            if (!hasStartedPhaseTransition)
            {
                hasLaunchedLaser = false;
                if (Laser != null)
                    Laser.Kill();

                frameGlowmaskMouth = 1;
                frameGlowmaskEye = 2;
                NPC.dontTakeDamage = true;
                ResetLaserTimer();
                ResetProjectileTimer();
                ai = 0;
                LaserTimer = 0;

                Vector2 shootPos = NPC.Center - new Vector2(20, -105);
                Vector2 shootVel = new Vector2(-1, 10) - shootPos;//+ new Vector2(Main.rand.NextFloat(-accuracy, accuracy), Main.rand.NextFloat(-accuracy, accuracy)); 
                shootVel.Normalize();
                shootVel *= 3.5f;

                Projectile.NewProjectile(new EntitySource_Misc(""), shootPos.X, shootPos.Y, shootVel.X, 0, ModContent.ProjectileType<Projectiles.BigBossLaser>(), NPC.damage / 2, 5f);
                hasStartedPhaseTransition = true;
                NPC.netUpdate = true;
            }

            //Charge the Laser
            if ((double)NPC.ai[0] <= (Main.expertMode ? 300f : 360f))
            {
                LaserChargingFlickeringTimer++;
                //Blinking Mouth Sign
                if (LaserChargingFlickeringTimer >= LaserChargingFlickeringTimerMax)
                {
                    LaserChargingFlickeringTimerMax = MathHelper.Lerp(Main.expertMode ? 40f : 50f, 10f, Main.expertMode ? NPC.ai[0] / 240f : NPC.ai[0] / 300f);
                    frameGlowmaskMouth = frameGlowmaskMouth == 0 ? 1 : 0;
                    if (frameGlowmaskMouth == 0)
                        SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Boss/LivingTrapBossLaser")
                        {
                            Volume = 0.5f,
                            Pitch = MathHelper.Lerp(-1f, 1f, Main.expertMode ? NPC.ai[0] / 300f : NPC.ai[0] / 360f)
                        });
                    LaserChargingFlickeringTimer = 0;
                }
            }

            // Feedback for shooting Laser
            if ((double)NPC.ai[0] >= (Main.expertMode ? 300f : 360f) && !hasLaunchedLaser)
            {
                foreach (Tile tile in LivingTrapBossArenaProtector.ArenaTiles)
                    tile.HasTile = false;
                player.GetModPlayer<CameraManager>().Shake(100, 200f);
                hasLaunchedLaser = true;
            }

            // Start Phase 2
            if ((double)NPC.ai[0] >= (Main.expertMode ? 300f : 360f) + 200f)
            {
                NPC.dontTakeDamage = false;
                ai = 0;
                frameGlowmaskMouth = 1;
                hasFinishedPhaseTransition = true;
                ResetLaserTimer();

            }

        }

        public override void AI()
        {
            // Spawn the Hand
            if (!doOnce)
            {
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X + NPC.width - 90, (int)NPC.position.Y + NPC.height, ModContent.NPCType<LivingTrapBossHand>());
                BossArenaLocation = new Vector2(LivingTrapBossArenaProtector.ArenaCoordinates[0], LivingTrapBossArenaProtector.ArenaCoordinates[2]).ToWorldCoordinates();
                doOnce = true;
            }

            // Reset projectile and laser timers
            if (!hasChangedProjectileAttackTimer)
                ResetProjectileTimer();

            if (!hasChangedLaserAttackTimer)
                ResetLaserTimer();


            NPC.TargetClosest(true);
            player = Main.player[NPC.target];

            Vector2 target = NPC.HasPlayerTarget ? player.Center : Main.npc[NPC.target].Center;

            NPC.rotation = 0.0f;
            NPC.netAlways = true;
            NPC.TargetClosest(true);

            // Despawn and respawn the tiles
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                NPC.direction = 1;
                NPC.velocity.X = 0;
                foreach (Tile tile in LivingTrapBossArenaProtector.ArenaTiles)
                    tile.HasTile = true;
                if (NPC.timeLeft > 5)
                {
                    NPC.active = false;
                    NPC.timeLeft = 5;
                    return;
                }
            }
            if (NPC.active == false)
            {
                foreach (Tile tile in LivingTrapBossArenaProtector.ArenaTiles)
                    tile.HasTile = true;
            }

            ai++;
            NPC.ai[0] = (float)ai;

            // Walk pattern and phase transition
            if (NPC.life <= NPC.lifeMax / 2 && hasFinishedPhaseTransition)
            {
                walkingTimer++;
                NPC.ai[2] = walkingTimer;

                if (NPC.ai[2] <= 100)
                    NPC.velocity.X = -2;
                else if (NPC.ai[2] > 100 && NPC.ai[2] <= 200)
                    NPC.velocity.X = 0;
                else if (NPC.ai[2] > 200)
                    walkingTimer = 0;
            }
            else if (NPC.life <= NPC.lifeMax / 2 && !hasFinishedPhaseTransition)
                PhaseTransition();
            if (!hasFinishedPhaseTransition && hasStartedPhaseTransition)
                return;



            if (NPC.life > NPC.lifeMax / 2 || hasFinishedPhaseTransition)
            {
                LaserTimer++;
                NPC.ai[1] = (float)LaserTimer;
            }

            // Laser and Projectiles Attacks
            LaserAI();
            ProjectileAI(target);

            //Rage
            if (target.X >= NPC.position.X || target.X <= BossArenaLocation.X || target.Y <= NPC.Center.Y - 250 || target.Y >= NPC.Center.Y + 250)
            {
                Enraged = true;
                if ((double)NPC.ai[0] % 50 == 0)
                {
                    player.GetModPlayer<CameraManager>().Shake(10, 50f);

                    for (int i = 0; i <= 10; i++)
                        Projectile.NewProjectile(new EntitySource_Misc(""), target.X - 500f + i * 100f, target.Y - 1000, 0, 10, ModContent.ProjectileType<Projectiles.StalactiteProjectile>(), 70, 1f);
                }

            }
            else
                Enraged = false;

            // Kill all players if the boss reach the western wall
            if (Vector2.Distance(player.position, NPC.position) <= 2000f && NPC.position.X <= BossArenaLocation.X)
                player.KillMe(PlayerDeathReason.ByCustomReason("You've been crushed"), 9999, 0);



        }
        /* public override void FindFrame(int frameHeight)
         {
             if (frame == 0)
             {
                 NPC.frame.Y = 0;
             }
             else if (frame == 1)
             {
                 NPC.frame.Y = frameHeight;
             }
             else
             {
                 NPC.frame.Y = frameHeight * 2;
             }
         }*/


        // Draw
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPosition, Color drawColor)
        {
            int frameCount = 3;
            Asset<Texture2D> texture = Request<Texture2D>("UnveiledMystery/Enemies/Boss/LivingTrapBoss");
            int frameHeight = texture.Value.Height / frameCount;
            int startY = frameHeight * (frame);
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Value.Width, frameHeight);

            spriteBatch.Draw
            (
                texture.Value,
                NPC.Center - screenPosition,
                sourceRectangle,
                new Color(0, 0, 0, 0),
                NPC.rotation,
                NPC.Size / 2f,
                NPC.scale,
                SpriteEffects.None,
                0f
            );
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPosition, Color alphaColor)
        {
            int frameCountEye = 3;
            Asset<Texture2D> textureEye = Request<Texture2D>("UnveiledMystery/Enemies/Boss/LivingTrapBoss_Glow");
            int frameHeightEye = textureEye.Value.Height / frameCountEye;
            int startYEye = frameHeightEye * (frameGlowmaskEye);
            Rectangle sourceRectangleEye = new Rectangle(0, startYEye, textureEye.Value.Width, frameHeightEye);

            spriteBatch.Draw
            (
                textureEye.Value,
                NPC.Center - screenPosition,
                sourceRectangleEye,
                Color.White,
                NPC.rotation,
                NPC.Size / 2f,
                NPC.scale,
                SpriteEffects.None,
                0f
            );

            int frameCountMouth = 2;
            Asset<Texture2D> textureMouth = Request<Texture2D>("UnveiledMystery/Enemies/Boss/LivingTrapBoss_GlowMouth");
            int frameHeightMouth = textureMouth.Value.Height / frameCountMouth;
            int startYMouth = frameHeightMouth * (frameGlowmaskMouth);
            Rectangle sourceRectangleMouth = new Rectangle(0, startYMouth, textureMouth.Value.Width, frameHeightMouth);

            spriteBatch.Draw
            (
                textureMouth.Value,
                NPC.Center - screenPosition,
                sourceRectangleMouth,
                Color.White,
                NPC.rotation,
                NPC.Size / 2f,
                NPC.scale,
                SpriteEffects.None,
                0f
            );

        }
    }
}
