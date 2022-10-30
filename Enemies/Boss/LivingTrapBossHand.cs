﻿using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using System;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.ID;

namespace UnveiledMystery.Enemies.Boss
{
    public class LivingTrapBossHand : ModNPC
    {
        private int maxStalactite = Main.expertMode ? 5 : 3;
        private int timerAttackStart = 0;
        private int timerAttackStartMax = Main.expertMode ? 240 : 300;
        private int timerBeforeSmash = 0;
        private int timerBeforeSmashMax = Main.expertMode ? 150 : 200;
        private int stalactiteCooldown = 0;
        private int handCoolOff = 0;
        private bool hasChangedAttackTimer = false;
        private bool hasGeneratedStalactite = false;
        private bool doOnce = false;
        private NPC Head;
        private LivingTrapBoss HeadScript;
        private List<NPC> Stalactites = new List<NPC>();
        private float originalRotation;
        private List<StalactiteNPC> StalactitesScript = new List<StalactiteNPC>();
        bool walkingStep = false;
        bool changeStep = false;


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
        private void PatternFirstPhase(Vector2 target, Vector2 startPosition)
        {

            timerAttackStart++;
            NPC.ai[0] = timerAttackStart;
            // Vertically align itself to the targetted player
            if (timerAttackStart >= timerAttackStartMax)
            {
                if (timerBeforeSmash < timerBeforeSmashMax)
                {
                    SoundEngine.PlaySound(SoundID.Item13);
                    timerBeforeSmash++;
                    NPC.ai[1] = timerBeforeSmash;
                    float playerHeight = target.Y;
                    Vector2 moveTo = new Vector2(Head.Center.X - 100, playerHeight);
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
                    if (NPC.Center.X <= Head.Center.X - 700)
                    {
                        SoundEngine.PlaySound(SoundID.Item70);
                        foreach (StalactiteNPC stalactite in StalactitesScript)
                            stalactite.Fall();

                        Stalactites.Clear();
                        StalactitesScript.Clear();
                        NPC.velocity = new Vector2(0, 0);
                        hasChangedAttackTimer = false;
                        HeadScript.player.GetModPlayer<CameraManager>().Shake(20, 50f);
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
        private void PatternSecondPhase(Vector2 target, Vector2 startPosition)
        {
            // Despawn the Stalactites on Phase Transition
            if (!HeadScript.hasFinishedPhaseTransition)
            {
                foreach (NPC stalactite in Stalactites)
                {
                    stalactite.life = 0;
                    stalactite.checkDead();
                    stalactite.active = false;
                }
                Stalactites.Clear();
                StalactitesScript.Clear();
                Replace(startPosition);
            }
            else if (HeadScript.hasFinishedPhaseTransition)
            {
                timerAttackStart++;
                NPC.ai[0] = timerAttackStart;

                if (NPC.ai[0] >= timerAttackStartMax)
                {
                    timerBeforeSmash++;
                    NPC.ai[1] = timerBeforeSmash;
                    // Allign itself horizontally to the targetted player
                    if (NPC.ai[1] < timerBeforeSmashMax)
                    {
                        SoundEngine.PlaySound(SoundID.Item13);
                        walkingStep = false;
                        int flameNumber = NPC.velocity.X > 0 ? 2 : 7;
                        for (int i = 0; i <= flameNumber; i++)
                            Dust.NewDust(NPC.position + new Vector2(NPC.width - 5, 10), 0, 50, 6, 10);

                        float playerX = target.X;
                        Vector2 moveTo = new Vector2(playerX, target.Y - 300f);
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
                                HeadScript.player.GetModPlayer<CameraManager>().Shake(20, 50f);
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

                    }
                }
                else
                    Replace(startPosition);
            }
            else
            {
                Replace(startPosition);
                hasChangedAttackTimer = false;

            }
        }

        // When hand is not attacking, place itself near the Boss' Head and do a little walking animation
        private void Replace(Vector2 startPosition)
        {
            Vector2 move = startPosition - NPC.Center;
            float speed = 5f;
            float magnitude = (float)Math.Sqrt(move.X * move.X + move.Y * move.Y);
            if (magnitude > speed)
            {
                move *= speed / magnitude;
            }
            NPC.velocity = move;
            if (HeadScript.hasFinishedPhaseTransition)
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
            }


        }

        // Place 3 stalactites
        private void GenerateNewStalactites()
        {
            for (int i = 0; i <= maxStalactite - 1; ++i)
            {
                NPC.NewNPC(NPC.GetSource_FromAI(), Main.rand.Next((int)Head.position.X - 650, (int)Head.position.X - 50), (int)Head.position.Y + 40, ModContent.NPCType<StalactiteNPC>());
                NPC.netUpdate = true;
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
                        stalactite.life = 0;
                        stalactite.checkDead();
                        stalactite.active = false;
                        Stalactites.Remove(stalactite);
                        StalactitesScript.Remove((StalactiteNPC)stalactite.ModNPC);
                        int replacedStalactite = NPC.NewNPC(NPC.GetSource_FromAI(), Main.rand.Next((int)Head.position.X - 650, (int)Head.position.X - 50), (int)Head.position.Y + 40, ModContent.NPCType<StalactiteNPC>());
                        Stalactites.Add(Main.npc[replacedStalactite]);
                        StalactitesScript.Add((StalactiteNPC)Main.npc[replacedStalactite].ModNPC);
                        NPC.netUpdate = true;
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
                originalRotation = NPC.rotation;
                Head = Main.npc.FirstOrDefault(x => x.active && x.type == ModContent.NPCType<LivingTrapBoss>());
                HeadScript = (LivingTrapBoss)Head.ModNPC;
                doOnce = true;
                GenerateNewStalactites();
            }

            if (!hasGeneratedStalactite)
                GenerateNewStalactites();

            if (!hasChangedAttackTimer)
            {
                timerAttackStart = 0;
                timerBeforeSmash = 0;
                hasChangedAttackTimer = true;
            }
            Vector2 startPosition = new Vector2(Head.position.X + Head.width - NPC.width / 2, Head.position.Y + Head.height - NPC.height / 2);

            // Disappear when there is no head anymore
            if (Head == null || !Head.active || Head.life <= 0)
            {
                NPC.life = 0;
                NPC.checkDead();
                NPC.active = false;
            }
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];
            Vector2 target = NPC.HasPlayerTarget ? player.Center : Main.npc[NPC.target].Center;

            if (Head.life > Head.lifeMax / 2 && !HeadScript.hasStartedPhaseTransition && !HeadScript.hasFinishedPhaseTransition)
                PatternFirstPhase(target, startPosition);
            else
                PatternSecondPhase(target, startPosition);
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
    }
}
