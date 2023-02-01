using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.Localization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnveiledMystery.Tiles;
using Terraria.GameContent.ItemDropRules;
using UnveiledMystery.Items;

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

        ProjectileType type;
        float ProjectileAITimer
        {
            get => NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        private Vector2 BossArenaLeftBorder;
        float LaserAITimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        float WalkingAITimer
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        int ProjectileAITimerMax;
        private bool hasChangedProjectileAITimerMax = false;
        private bool hasChangedLaserAITimer = false;
        int LaserAttackStartTime;

        private float LaserChargingFlickeringTimer = 0;
        private float LaserChargingFlickeringTimerMax = 50;

        public bool HasFinishedPhaseTransition = false;
        public bool HasStartedPhaseTransition = false;
        private bool hasLaunchedLaser = false;

        private Projectile laser;
        public Player Player;
        public bool Enraged = false;

        //Animation
        private int frame = 0;
        public int FrameGlowmaskEye = 0;
        public int FframeGlowmaskMouth = 1;
        private bool doOnce = false;
        List<Player> PlayersInArena = new List<Player>();



        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            DisplayName.SetDefault("Living Trap");
            Main.npcFrameCount[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            NPC.netAlways = true;

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
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if(HasFinishedPhaseTransition)
                    ProjectileAITimerMax = Main.expertMode ? Main.rand.Next(50, 150) : Main.rand.Next(100, 200);
                else
                    ProjectileAITimerMax = Main.expertMode ? Main.rand.Next(100, 200) : Main.rand.Next(150, 250);

                type = (ProjectileType)Main.rand.Next(1, 3);
            }
            NPC.netUpdate = true;

            hasChangedProjectileAITimerMax = true;
        }

        private void ResetLaserTimer()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                LaserAttackStartTime = Main.expertMode ? Main.rand.Next(100, 200) : Main.rand.Next(150, 250);

            }

            hasChangedLaserAITimer = true;
            LaserChargingFlickeringTimerMax = Main.expertMode ? 40 : 50;
            LaserChargingFlickeringTimer = 0;
            FframeGlowmaskMouth = 1;
            laser = null;
            NPC.netUpdate = true;

        }
        private void LaserAI()
        {

            // Reload the Laser
            if ((double)LaserAITimer >= LaserAttackStartTime + (Main.expertMode ? 150f : 200f))
            {
                LaserAITimer = 0;
                hasChangedLaserAITimer = false;
            }
            // Shoot the Laser
            else if ((double)LaserAITimer == LaserAttackStartTime)
            {

                Vector2 shootPos = NPC.Center - new Vector2(20, -145);
                Vector2 shootVel = new Vector2(-1, 10) - shootPos;//+ new Vector2(Main.rand.NextFloat(-accuracy, accuracy), Main.rand.NextFloat(-accuracy, accuracy)); 
                shootVel.Normalize();
                shootVel *= 3.5f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    laser = Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), shootPos.X, shootPos.Y, shootVel.X, 0, ModContent.ProjectileType<Projectiles.LivingTrapLaser>(), NPC.damage / 2, 5f)];
            }
            // Charge the Laser
            if ((double)LaserAITimer >= LaserAttackStartTime)
            {
                LaserChargingFlickeringTimer++;
                //Blinking Mouth Sign
                if (LaserChargingFlickeringTimer >= LaserChargingFlickeringTimerMax)
                {
                    LaserChargingFlickeringTimerMax = MathHelper.Lerp(Main.expertMode ? 40f : 50f, 10f, Main.expertMode ? LaserAITimer / (LaserAttackStartTime + 50f) : LaserAITimer / (LaserAttackStartTime + 100f));
                    FframeGlowmaskMouth = FframeGlowmaskMouth == 0 ? 1 : 0;
                    if (FframeGlowmaskMouth == 0)
                        SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Boss/LivingTrapBossLaser")
                        {
                            Volume = 0.5f,
                            Pitch = MathHelper.Lerp(-1f, 1f, Main.expertMode ? LaserAITimer / (LaserAttackStartTime + 150f) : LaserAITimer / (LaserAttackStartTime + 210f))
                        });
                    LaserChargingFlickeringTimer = 0;
                }
            }
        }
        private void ProjectileAI(Vector2 target)
        {
            int distance = (int)Vector2.Distance(target, NPC.Center);
            if ((double)ProjectileAITimer < ProjectileAITimerMax)
            {
                FrameGlowmaskEye = 2;
            }
            // Visual Attack Sign
            else if ((double)ProjectileAITimer >= ProjectileAITimerMax && (double)ProjectileAITimer < ProjectileAITimerMax + 120)
            {
                SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Boss/LivingTrapBossProjectile")
                {
                    MaxInstances = 1,
                    SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                });

                if (type == ProjectileType.BOUNCING)
                {
                    FrameGlowmaskEye = 0;
                    Dust.NewDust(NPC.Center - new Vector2(0, 140), 30, 30, 131);
                }
                else if (type == ProjectileType.TRIPLE)
                {
                    FrameGlowmaskEye = 1;
                    Dust.NewDust(NPC.Center - new Vector2(0, 140), 30, 30, 130);
                }
            }

            // Shoot
            else if ((double)ProjectileAITimer >= ProjectileAITimerMax + 120)
            {
                ShootProjectile(target);
                hasChangedProjectileAITimerMax = false;
                ProjectileAITimer = 0;
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
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), shootPos.X, shootPos.Y, RotatedVel.X * 0.75f, RotatedVel.Y * 0.75f, ProjectileID.EyeLaser, NPC.damage / 3, 5f);
                        }
                        break;

                    }
                // Bouncing Laser
                case ProjectileType.BOUNCING:
                    {
                        float rotation;
                        if (Main.expertMode)
                        {
                            rotation = MathHelper.ToRadians(Main.rand.NextFloat(0, 30));
                            NPC.netUpdate = true;

                        }
                        else
                            rotation = Main.expertMode ? MathHelper.ToRadians(Main.rand.NextFloat(0, 30)) : MathHelper.ToRadians(0);
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
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), shootPos.X, shootPos.Y, Main.expertMode ? RotatedVel.X * 1.5f : shootVel.X * 1.5f, Main.expertMode ? RotatedVel.Y * 1.5f : shootVel.Y * 1.5f, ModContent.ProjectileType<Projectiles.LivingTrapBouncingProjectile>(), NPC.damage / 3, 5f);

                        }
                    }
                    break;
            }
        }

        // Do what happen when boss is half hp
        private void PhaseTransition()
        {
            if (!HasStartedPhaseTransition)
            {
                hasLaunchedLaser = false;
                if (laser != null)
                    laser.Kill();

                FframeGlowmaskMouth = 1;
                FrameGlowmaskEye = 2;
                NPC.dontTakeDamage = true;
                ResetLaserTimer();
                ResetProjectileTimer();
                ProjectileAITimer = 0;
                LaserAITimer = 0;

                Vector2 shootPos = NPC.Center - new Vector2(20, -105);
                Vector2 shootVel = new Vector2(-1, 10) - shootPos;//+ new Vector2(Main.rand.NextFloat(-accuracy, accuracy), Main.rand.NextFloat(-accuracy, accuracy)); 
                shootVel.Normalize();
                shootVel *= 3.5f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), shootPos.X, shootPos.Y, shootVel.X, 0, ModContent.ProjectileType<Projectiles.LivingTrapBigLaser>(), NPC.damage / 2, 5f);
                HasStartedPhaseTransition = true;
            }

            //Charge the Laser
            if ((double)ProjectileAITimer <= (Main.expertMode ? 200f : 260f))
            {
                LaserChargingFlickeringTimer++;
                //Blinking Mouth Sign
                if (LaserChargingFlickeringTimer >= LaserChargingFlickeringTimerMax)
                {
                    LaserChargingFlickeringTimerMax = MathHelper.Lerp(Main.expertMode ? 40f : 50f, 10f, Main.expertMode ? ProjectileAITimer / 140f : ProjectileAITimer / 200f);
                    FframeGlowmaskMouth = FframeGlowmaskMouth == 0 ? 1 : 0;
                    if (FframeGlowmaskMouth == 0)
                        SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Boss/LivingTrapBossLaser")
                        {
                            Volume = 0.5f,
                            Pitch = MathHelper.Lerp(-1f, 1f, Main.expertMode ? ProjectileAITimer / 200f : ProjectileAITimer / 260f)
                        });
                    LaserChargingFlickeringTimer = 0;
                }
            }

            // Feedback for shooting Laser
            if ((double)ProjectileAITimer >= (Main.expertMode ? 200f : 260f) && !hasLaunchedLaser)
            {
                foreach (Tile tile in LivingTrapBossArenaProtector.ArenaTiles)
                    tile.HasTile = false;
                Player.GetModPlayer<CameraManager>().Shake(100, 200f);
                hasLaunchedLaser = true;
                FframeGlowmaskMouth = 0;
            }

            // Start Phase 2
            if ((double)ProjectileAITimer >= (Main.expertMode ? 200f : 260f) + 200f)
            {
                NPC.dontTakeDamage = false;
                ProjectileAITimer = 0;
                HasFinishedPhaseTransition = true;
            }

        }

        public override void AI()
        {
            if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                /*ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("timer = " + ProjectileAttackTimer), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("frameEye =  " + frameGlowmaskEye), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("projectileType " + type), Color.White);
                //ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("LaserTimerStart " + LaserTimerStart), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("NPC.AI[0] " + NPC.ai[0]), Color.White);
                //ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("NPC.AI[1] " + NPC.ai[1]), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("hasLaunchedLaser = " + hasLaunchedLaser), Color.White);*/

            }

            NPC.TargetClosest(true);

            // Spawn the Hand
            if (!doOnce)
            {
                for (int x = 0; x < Main.tile.Width; x++)
                {
                    for (int y = 0; y < Main.tile.Height; y++)
                    {
                        Tile tile= Main.tile[x,y];
                        if (tile.TileType == ModContent.TileType<LivingTrapArenaTrapDoor_Tile>())
                        {
                            if (tile.TileFrameX != 0 && tile.TileFrameX != 18)
                                tile.TileFrameX -= 36;
                            Main.tileSolid[tile.TileType] = true;

                        }
                    }
                }
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket myPacket = Mod.GetPacket();

                    myPacket.Write((byte)UnveiledMystery.MessageType.SpawnNPCByNPC);
                    myPacket.Write((byte)NPC.whoAmI);
                    myPacket.Write((int)NPC.position.X + NPC.width - 90);
                    myPacket.Write((int)NPC.position.Y + NPC.height);
                    myPacket.Write((int)ModContent.NPCType<LivingTrapBossHand>());
                    myPacket.Send();
                }
                else
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X + NPC.width - 90, (int)NPC.position.Y + NPC.height, ModContent.NPCType<LivingTrapBossHand>());
                BossArenaLeftBorder = new Vector2(LivingTrapBossArenaProtector.ArenaCoordinates[0], LivingTrapBossArenaProtector.ArenaCoordinates[2]).ToWorldCoordinates();

                doOnce = true;
            }
            PlayersInArena = LivingTrapBossArenaProtector.PlayersInArena;
            if (PlayersInArena.Count != 0)
            {
                if (PlayersInArena.Count == 1)
                    Player = Main.player[NPC.target];
                else
                    Player = PlayersInArena[Main.rand.Next(0, PlayersInArena.Count)];
            }
            // Reset projectile and laser timers
            if (!hasChangedProjectileAITimerMax)
                ResetProjectileTimer();

            if (!hasChangedLaserAITimer && !HasFinishedPhaseTransition)
                ResetLaserTimer();




            Vector2 target = Player != null ? Player.Center : Main.npc[NPC.target].Center;

            NPC.rotation = 0.0f;

            // Despawn and respawn the tiles
            if(PlayersInArena.All(p => !p.active || p.dead) && PlayersInArena.Count == 0)
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

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                ProjectileAITimer++;
            }

            // Walk pattern and phase transition
            if (NPC.life <= NPC.lifeMax / 2 && HasFinishedPhaseTransition)
            {
                WalkingAITimer++;

                if (WalkingAITimer <= 100)
                    NPC.velocity.X = -2;
                else if (WalkingAITimer > 100 && WalkingAITimer <= 200)
                    NPC.velocity.X = 0;
                else if (WalkingAITimer > 200)
                    WalkingAITimer = 0;
            }
            else if (NPC.life <= NPC.lifeMax / 2 && !HasFinishedPhaseTransition)
                PhaseTransition();
            if (!HasFinishedPhaseTransition && HasStartedPhaseTransition)
                return;



            if (NPC.life > NPC.lifeMax / 2 || HasFinishedPhaseTransition)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    LaserAITimer++;
                    LaserAITimer = (float)LaserAITimer;

                }
            }

            // Laser and Projectiles Attacks
            if(!HasFinishedPhaseTransition)
                LaserAI();

            ProjectileAI(target);

            //Rage
            if (target.X >= NPC.position.X || target.X <= BossArenaLeftBorder.X || target.Y <= NPC.Center.Y - 250 || target.Y >= NPC.Center.Y + 250)
            {
                Enraged = true;
                if ((double)ProjectileAITimer % 50 == 0)
                {
                    Player.GetModPlayer<CameraManager>().Shake(10, 50f);

                    for (int i = 0; i <= 10; i++)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), target.X - 500f + i * 100f, target.Y - 1000, 0, 10, ModContent.ProjectileType<Projectiles.StalactiteProjectile>(), 70, 1f);

                    }
                }

            }
            else
                Enraged = false;

            // Kill all players if the boss reach the western wall
            if (Vector2.Distance(Player.position, NPC.position) <= 2000f && NPC.position.X <= BossArenaLeftBorder.X)
                Player.KillMe(PlayerDeathReason.ByCustomReason("You've been crushed"), 9999, 0);



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

        public override void SendExtraAI(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(ProjectileAITimerMax);
            binaryWriter.Write((int)type);
            binaryWriter.Write(LaserAttackStartTime);

        }

        public override void ReceiveExtraAI(BinaryReader binaryReader)
        {
            ProjectileAITimerMax = binaryReader.Read();
            type = (ProjectileType)binaryReader.Read();
            LaserAttackStartTime = binaryReader.Read();

        }

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
            Asset<Texture2D> textureEye = Request<Texture2D>("UnveiledMystery/Enemies/Boss/LivingTrapBoss_GlowEye");
            int frameHeightEye = textureEye.Value.Height / frameCountEye;
            int startYEye = frameHeightEye * (FrameGlowmaskEye);
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
            int startYMouth = frameHeightMouth * (FframeGlowmaskMouth);
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

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Do NOT misuse the ModifyNPCLoot and OnKill hooks: the former is only used for registering drops, the latter for everything else

            // Add the treasure bag using ItemDropRule.BossBag (automatically checks for expert mode)
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<LivingTrapBossBag>()));

            // TODO : Trophy
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Placeable.Furniture.MinionBossTrophy>(), 10));

            // TODO : Master Mode Boss Relic
            //npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.MinionBossRelic>()));

            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<LivingTrapMinion_Item>()));

            // All our drops here are based on "not expert", meaning we use .OnSuccess() to add them into the rule, which then gets added
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

            // Notice we use notExpertRule.OnSuccess instead of npcLoot.Add so it only applies in normal mode
            // Boss masks are spawned with 1/7 chance
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<TurretCraftingStation_Item>()));

            // Finally add the leading rule
            npcLoot.Add(notExpertRule);
        }
    }
}
