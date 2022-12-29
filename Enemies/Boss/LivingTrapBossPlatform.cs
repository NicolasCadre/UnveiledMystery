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
using CollisionLib;

namespace UnveiledMystery.Enemies.Boss
{
    public class LivingTrapBossPlatform : ModNPC
    {
        public CollisionSurface collider = null;
        private float timer;
        private const float TIMERMAX = 600;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 10;

            NPC.aiStyle = -1;
            NPC.friendly = true;
            NPC.dontTakeDamage = true;
            NPC.lifeMax = 1;
            NPC.damage = 0;

            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.lavaImmune = true;

        }


        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        public override bool PreAI()
        {
            if (collider == null) //initializes the collision surfaces if null, this happens before everything else so that the logic works starting the first frame
            {
                collider = new CollisionSurface(NPC.TopLeft, NPC.TopRight, new int[] { 2, 0, 0,0 });
            }


            return base.PreAI();
        }

        public override void AI()
        {
            timer++;
            if (timer >= TIMERMAX)
            {
                NPC.life = 0;
                NPC.checkDead();
                NPC.active = false;
            }
            NPC.velocity = new Vector2(1.5f, 0);
            if (collider != null) 
            {
                collider.Update();
                collider.endPoints[0] = NPC.Center + (NPC.TopLeft - NPC.Center).RotatedBy(NPC.rotation);
                collider.endPoints[1] = NPC.Center + (NPC.TopRight - NPC.Center).RotatedBy(NPC.rotation);
            }
        }

        public override void PostAI()
        {
            if (collider != null)
            {
                collider.PostUpdate(); 
            }
        }
    }
}
