using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using System;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.ID;
using System.IO;
using Terraria.Chat;
using Terraria.Localization;

namespace UnveiledMystery.Enemies.Boss
{
    public class LivingTrapBossHand : ModNPC
    {
        enum Direction
        {
            UP = 1,
            DOWN = 2
        }
        private Direction dir = Direction.UP;
        private int maxStalactites = Main.expertMode ? 5 : 3;
        int timerAttack
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        private int timerAttackStartMax = Main.expertMode ? 240 : 300;
        private int timerShootPlatormMax = 100;
        private int timerBeforeSmash = 0;
        private int timerBeforeSmashMax = Main.expertMode ? 150 : 200;
        private int stalactiteCooldown = 0;
        private bool hasChangedTimerAttack = false;
        private bool hasGeneratedStalactite = false;
        private bool doOnce = false;
        private NPC LivingTrapHead;
        private LivingTrapBoss LivingTrapHeadScript;
        public List<NPC> Stalactites = new List<NPC>();
        public List<StalactiteNPC> StalactitesScript = new List<StalactiteNPC>();
        List<Player> PlayersInArena = new List<Player>();
        Player choosenPlayer = null;
        int[] StalactiteX = new int[5]; 

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Living Trap Hand");
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 70;

            NPC.aiStyle = -1;
            NPC.dontTakeDamage = true;
            NPC.lifeMax = 1;
            NPC.damage = 100;

            NPC.value = Item.buyPrice(gold: 10);

            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.lavaImmune = true;

        }


        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        // Phase 1
        private void PatternFirstPhase(Vector2 startPosition)
        {

            timerAttack++;
            // Vertically align itself to the targetted player
            if (timerAttack >= timerAttackStartMax)
            {
                if (timerBeforeSmash < timerBeforeSmashMax)
                {
                    SoundEngine.PlaySound(SoundID.Item13);
                    timerBeforeSmash++;
                    NPC.ai[1] = timerBeforeSmash;
                    float playerHeight = Main.player[choosenPlayer.whoAmI].position.Y;
                    Vector2 moveTo = new Vector2(LivingTrapHead.Center.X - 100, playerHeight);
                    Vector2 move = moveTo - NPC.Center;
                    float speed = MathHelper.Lerp(2, 5, Vector2.Distance(new Vector2(NPC.Center.X, playerHeight), NPC.Center) / 200);
                    float magnitude = (float)Math.Sqrt(move.X * move.X + move.Y * move.Y);
                    if (magnitude > speed)
                    {
                        move *= speed / magnitude;
                    }
                    NPC.velocity = move;
                    for (int i = 0; i <= 5; i++)
                        Dust.NewDust(NPC.position + new Vector2(NPC.width - 5, 10), 0, 50, 6, 10);
                }
                // Rush to the left side of the arena
                else
                {
                    SoundEngine.PlaySound(SoundID.Item20 with { MaxInstances = 1 });
                    NPC.velocity.X = -15;
                    NPC.velocity.Y = 0;
                    for (int i = 0; i <= 20; i++)
                        Dust.NewDust(NPC.position + new Vector2(NPC.width - 5, 0), 0, 70, 6, 1);
                    // Smash the wall
                    if (NPC.Center.X <= LivingTrapHead.Center.X - 700)
                    {
                        SoundEngine.PlaySound(SoundID.Item70);
                        foreach (StalactiteNPC stalactite in StalactitesScript)
                            stalactite.Fall();

                        Stalactites.Clear();
                        StalactitesScript.Clear();
                        NPC.velocity = new Vector2(0, 0);
                        hasChangedTimerAttack = false;
                        foreach(Player p in PlayersInArena)
                            p.GetModPlayer<CameraManager>().Shake(20, 50f);
                        for (int i = 0; i <= 10; i++)
                            Dust.NewDust(NPC.Center - new Vector2(50, 0), 10, 70, 1);
                        stalactiteCooldown = 0;
                    }
                }
            }
            else
            {
                // Respawns the stalactites NPCs
                if (Stalactites.Count == 0)
                {
                    stalactiteCooldown++;
                    NPC.ai[2] = stalactiteCooldown;
                    if ((double)NPC.ai[2] >= 180)
                        hasGeneratedStalactite = false;
                }
                Replace(startPosition);

            }
        }

        // Phase 2
        private void PatternSecondPhase(Vector2 startPosition)
        {
            // Despawn the Stalactites on Phase Transition
            if (!LivingTrapHeadScript.HasFinishedPhaseTransition)
            {
                foreach (NPC stalactite in Stalactites)
                {
                    stalactite.life = 0;
                    stalactite.checkDead();
                    stalactite.active = false;
                }
                Stalactites.Clear();
                StalactitesScript.Clear();
                MovePhase2();
            }
            else if (LivingTrapHeadScript.HasFinishedPhaseTransition)
            {
                NPC.direction = 1;
                NPC.spriteDirection = 1;

                timerAttack++;
                MovePhase2();
                if (timerAttack >= timerShootPlatormMax)
                {

                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<LivingTrapBossPlatform>());
                    timerAttack = 0;
                    dir = (Direction)Main.rand.Next(1, 3);
                    /*timerBeforeSmash++;
                    NPC.ai[1] = timerBeforeSmash;
                    // Allign itself horizontally to the targetted player
                    if (NPC.ai[1] < timerBeforeSmashMax)
                    {
                        SoundEngine.PlaySound(SoundID.Item13);
                        walkingStep = false;
                        int flameNumber = NPC.velocity.X > 0 ? 2 : 7;
                        for (int i = 0; i <= flameNumber; i++)
                            Dust.NewDust(NPC.position + new Vector2(NPC.width - 5, 10), 0, 50, 6, 10);

                        float playerX = Main.player[choosenPlayer.whoAmI].position.X;
                        Vector2 moveTo = new Vector2(playerX, Main.player[choosenPlayer.whoAmI].position.Y - 300f);
                        Vector2 move = moveTo - NPC.Center;
                        float speed = MathHelper.Lerp(2, 5, Vector2.Distance(new Vector2(playerX, NPC.Center.Y), NPC.Center) / 200);
                        float magnitude = (float)Math.Sqrt(move.X * move.X + move.Y * move.Y);
                        if (magnitude > speed)
                        {
                            move *= speed / magnitude;
                        }
                        NPC.velocity = move;
                        NPC.rotation = originalRotation;

                    }
                    // Smash the floor
                    else
                    {
                        SoundEngine.PlaySound(SoundID.Item20 with { MaxInstances = 1 });
                        NPC.velocity.X = 0;
                        NPC.velocity.Y = 10;
                        for (int i = 0; i <= 10; i++)
                            Dust.NewDust(NPC.position - new Vector2(0, NPC.height / 2 - 20), NPC.width, 0, 6, 0, -10);
                        if (NPC.Center.Y >= (int)Head.position.Y + Head.height - (NPC.height / 2))
                        {
                            handCoolOff++;
                            NPC.ai[2] = handCoolOff;
                            if (NPC.ai[2] == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item70);
                                foreach (Player p in PlayersInArena)
                                    p.GetModPlayer<CameraManager>().Shake(20, 50f);
                                for (int i = 0; i <= 20; i++)
                                    Dust.NewDust(NPC.Center - new Vector2(0, 0), 250, 0, 1);
                            }
                            NPC.velocity = new Vector2(0, 0);
                        }
                        if (NPC.ai[2] >= 60f)
                        {
                            handCoolOff = 0;
                            NPC.ai[2] = 0;
                            walkingStep = false;
                            hasChangedAttackTimer = false;
                        }

                    }*/
                }
                //else
                //Replace(startPosition);
            }
            else
            {
                MovePhase2();
                hasChangedTimerAttack = false;
                dir = Direction.UP;
            }
        }
        private void MovePhase2()
        {
            //Move the Hands during phase 2; in phase 1
            if (LivingTrapHeadScript.HasStartedPhaseTransition && !LivingTrapHeadScript.HasFinishedPhaseTransition)
                NPC.position = new Vector2(LivingTrapHead.Center.X - 1000, LivingTrapHead.Center.Y);
            if (NPC.Center.Y - LivingTrapBossArenaProtector.ArenaCoordinates[2] * 16 <= 80f)
                dir = Direction.DOWN;
            if (LivingTrapBossArenaProtector.ArenaCoordinates[3] * 16 - 100 - NPC.Center.Y <= 50f)
                dir = Direction.UP;
            Vector2 move = new Vector2(NPC.Center.X - 1.5f, dir == Direction.UP ? NPC.Center.Y - 1 : NPC.Center.Y + 1) - NPC.Center;

            Move(move);
        }

        private void Move(Vector2 movement)
        {
            float speed = 5f;
            float magnitude = (float)Math.Sqrt(movement.X * movement.X + movement.Y * movement.Y);
            if (magnitude > speed)
            {
                movement *= speed / magnitude;
            }
            NPC.velocity = movement;

        }
        // When hand is not attacking, place itself near the Boss' Head and do a little walking animation
        private void Replace(Vector2 startPosition)
        {
            //Place the hand near the head
            Vector2 move = startPosition - NPC.Center;
            Move(move);
            /*if (HeadScript.hasFinishedPhaseTransition)
            {

                
            if (Head.ai[2] <= 10 && !changeStep)
                {
                    walkingStep = !walkingStep;
                    changeStep = true;
                }
                if (Head.ai[2] >= 100)
                    changeStep = false;
                if (Vector2.Distance(NPC.Center, startPosition) < 10)
                {
                    if (walkingStep)
                        NPC.rotation = Utils.AngleLerp(originalRotation, originalRotation + MathHelper.ToRadians(30), Math.Clamp(Head.ai[2] / 100, 0, 1));
                    else
                        NPC.rotation = Utils.AngleLerp(originalRotation + MathHelper.ToRadians(30), originalRotation, Math.Clamp(Head.ai[2] / 100, 0, 1));
                }
                else
                    NPC.rotation = originalRotation;
            }*/


        }

        // Place 3 stalactites
        private void GenerateNewStalactites()
        {
            for (int i = 0; i <= maxStalactites - 1; ++i)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    StalactiteX[i] = Main.rand.Next((int)LivingTrapHead.position.X - 650, (int)LivingTrapHead.position.X - 50);
                    NPC.netUpdate = true;
                }
            }
            for (int i = 0; i <= maxStalactites - 1; ++i)
            {

                if (Main.netMode != NetmodeID.MultiplayerClient && Main.netMode != NetmodeID.SinglePlayer)
                {
                    ModPacket myPacket = Mod.GetPacket();
                    myPacket.Write((byte)UnveiledMystery.MessageType.SpawnNPCByNPC);
                    myPacket.Write((byte)NPC.whoAmI);
                    myPacket.Write ((int)StalactiteX[i]);
                    myPacket.Write((int)LivingTrapHead.position.Y + 40);
                    myPacket.Write((int)ModContent.NPCType<StalactiteNPC>());
                    myPacket.Send();
                }
                else
                    NPC.NewNPC(NPC.GetSource_FromAI(), StalactiteX[i], (int)LivingTrapHead.position.Y + 40, ModContent.NPCType<StalactiteNPC>());
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].type == ModContent.NPCType<StalactiteNPC>())
                {
                    Stalactites.Add(Main.npc[i]);
                    StalactitesScript.Add((StalactiteNPC)Main.npc[i].ModNPC);
                }
            }
            SeparateStalactites();
        }

        private void SeparateStalactites()
        {

            while (!hasGeneratedStalactite)
            {
                foreach (NPC stalactite in Stalactites.ToList())
                {
                    if (Stalactites.Any(npc => Vector2.Distance(npc.position, stalactite.position) <= 20 && npc != stalactite))
                    {
                        int indexStalactiteX = Stalactites.IndexOf(stalactite);
                        stalactite.life = 0;
                        stalactite.checkDead();
                        stalactite.active = false;
                        Stalactites.Remove(stalactite);
                        StalactitesScript.Remove((StalactiteNPC)stalactite.ModNPC);
                        int replacedStalactite;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            StalactiteX[indexStalactiteX] = Main.rand.Next((int)LivingTrapHead.position.X - 650, (int)LivingTrapHead.position.X - 50);
                            NPC.netUpdate = true;

                        }
                        /*if (Main.netMode != NetmodeID.MultiplayerClient && Main.netMode != NetmodeID.SinglePlayer)
                        {

                            NPC.netUpdate = true;
                            ModPacket myPacket = Mod.GetPacket();
                            myPacket.Write((byte)UnveiledMystery.MessageType.ReplaceStalactite);
                            myPacket.Write((byte)NPC.whoAmI);
                            myPacket.Write((int)StalactiteX[indexStalactiteX]);
                            myPacket.Write((int)Head.position.Y + 40);
                            myPacket.Write((int)ModContent.NPCType<StalactiteNPC>());
                            myPacket.Send();
                        }
                        else
                        {*/
                            replacedStalactite = NPC.NewNPC(NPC.GetSource_FromAI(), StalactiteX[indexStalactiteX], (int)LivingTrapHead.position.Y + 40, ModContent.NPCType<StalactiteNPC>());
                            Stalactites.Add(Main.npc[replacedStalactite]);
                            StalactitesScript.Add((StalactiteNPC)Main.npc[replacedStalactite].ModNPC);
                        //}
                        continue;

                    }
                }
                bool isVerified = true;
                foreach (NPC stalactite in Stalactites)
                {
                    if (Stalactites.Any(npc => Vector2.Distance(npc.position, stalactite.position) <= 20 && npc != stalactite))
                        isVerified = false;
                }
                if (isVerified)
                    hasGeneratedStalactite = true;
            }

        }
        public override void AI()
        {
            // Link itself to the Boss' Head
            if (!doOnce)
            {
                LivingTrapHead = Main.npc.FirstOrDefault(x => x.active && x.type == ModContent.NPCType<LivingTrapBoss>());
                LivingTrapHeadScript = (LivingTrapBoss)LivingTrapHead.ModNPC;
                doOnce = true;
                GenerateNewStalactites();
            }

            if (!hasGeneratedStalactite)
                GenerateNewStalactites();

            if (!hasChangedTimerAttack)
            {
                timerAttack = 0;
                timerBeforeSmash = 0;
                hasChangedTimerAttack = true;
                PlayersInArena = LivingTrapBossArenaProtector.PlayersInArena;
                if (PlayersInArena.Count != 0)
                {
                    if (PlayersInArena.Count == 1)
                        choosenPlayer = Main.player[NPC.target];
                    else
                        choosenPlayer = PlayersInArena[Main.rand.Next(0, PlayersInArena.Count)];
                }
            }
            Vector2 startPosition = new Vector2(LivingTrapHead.position.X + LivingTrapHead.width - NPC.width / 2, LivingTrapHead.position.Y + LivingTrapHead.height - NPC.height / 2);

            // Disappear when there is no head anymore
            if (LivingTrapHead == null || !LivingTrapHead.active || LivingTrapHead.life <= 0)
            {
                NPC.life = 0;
                NPC.checkDead();
                NPC.active = false;
            }

            if (LivingTrapHead.life > LivingTrapHead.lifeMax / 2 && !LivingTrapHeadScript.HasStartedPhaseTransition && !LivingTrapHeadScript.HasFinishedPhaseTransition)
                PatternFirstPhase(startPosition);
            else
                PatternSecondPhase(startPosition);
        }
        /*public override void SendExtraAI(BinaryWriter binaryWriter)
        {
            if(maxStalactite == 3)
            {
                binaryWriter.Write(StalactiteX[0]);
                binaryWriter.Write(StalactiteX[1]);
                binaryWriter.Write(StalactiteX[2]);
            }
            else if (maxStalactite == 5)
            {
                binaryWriter.Write(StalactiteX[0]);
                binaryWriter.Write(StalactiteX[1]);
                binaryWriter.Write(StalactiteX[2]);
                binaryWriter.Write(StalactiteX[3]);
                binaryWriter.Write(StalactiteX[4]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader binaryReader)
        {
            if (maxStalactite == 3)
            {
                StalactiteX[0] = binaryReader.Read();
                StalactiteX[1] = binaryReader.Read();
                StalactiteX[2] = binaryReader.Read();
            }
            else if (maxStalactite == 5)
            {
                StalactiteX[0] = binaryReader.Read();
                StalactiteX[1] = binaryReader.Read();
                StalactiteX[2] = binaryReader.Read();
                StalactiteX[3] = binaryReader.Read();
                StalactiteX[4] = binaryReader.Read();
            }
        }*/
        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
    }
}
