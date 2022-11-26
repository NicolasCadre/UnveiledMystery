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
using UnveiledMystery.Enemies.Boss;
using System;

namespace UnveiledMystery.Enemies
{
    enum Direction
    {
        UP,
        DOWN
    }
    public class MirrorGhost : ModNPC
    {

        float timerFloat = 60;
        float timerFloatMax = 120;
        Direction timerFloatDirection = Direction.UP;
        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 43;

            NPC.aiStyle = -1;
            NPC.dontTakeDamage = true;
            NPC.lifeMax = 1;
            NPC.friendly = true;

            NPC.value = Item.buyPrice(copper: 0);

            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.lavaImmune = true;

        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }


        public override void AI()
        {
            float factor = timerFloat / timerFloatMax;
            factor = MathHelper.SmoothStep(0, 1, factor);
            if (timerFloatDirection == Direction.UP)
                timerFloat++;
            else
                timerFloat--;
            if (timerFloat > timerFloatMax)
                timerFloatDirection = Direction.DOWN;
            else if (timerFloat < 0)
                timerFloatDirection = Direction.UP;


            float ghostBaseHeight = MathHelper.Lerp(Main.player[0].position.Y - 195f, Main.player[0].position.Y - 205f,factor);
            NPC.position.X = Main.player[0].position.X;
            NPC.position.Y = ghostBaseHeight;
            Vector2 Dustspeed = Main.rand.NextVector2Unit((float)MathHelper.Pi / 4, (float)MathHelper.Pi / 3) * Main.rand.NextFloat();
            Dust.NewDust(NPC.Center + new Vector2(0, NPC.height / 2), 0, 0, 59, Dustspeed.X *0.5f, Dustspeed.Y *0.5f);
        }
    }
}
