using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using UnveiledMystery.Enemies.Boss;
using Terraria.Chat;
using System.Reflection;
using NVorbis.Contracts;

namespace UnveiledMystery.Tiles
{
    public class BossSummonAltarTile : ModTile
    {

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            AddMapEntry(new Color(255, 255, 255), Language.GetText("Strange Altar"));
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3;
            TileObjectData.addTile(Type);
            ItemDrop = ModContent.ItemType<Items.RoomChoserItem>();
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 48, ModContent.ItemType<Items.BossSummonAltarItem>());
        }
        public override bool CanKillTile(int i, int j, ref bool blockDamaged)
        {
            return false;
        }

        // Spawn the Boss on the wall to the right of the arena
        public override bool RightClick(int i, int j)
        {
            Vector2 floorHeight;
            Vector2 BossLocation;
            float BossI;
            float BossJ;
            if (Main.tile[i + 2, j].TileType == ModContent.TileType<BossSummonAltarTile>())
                BossI = i + 21;
            else if (Main.tile[i + 1, j].TileType == ModContent.TileType<BossSummonAltarTile>())
                BossI = i + 20;
            else
                BossI = i + 19;

            if (Main.tile[i, j + 2].TileType == ModContent.TileType<BossSummonAltarTile>())
                BossJ = j + 2.5f;
            else if (Main.tile[i, j + 1].TileType == ModContent.TileType<BossSummonAltarTile>())
                BossJ = j + 1.5f;
            else
                BossJ = j + 0.5f;

            BossLocation = new Vector2(BossI, BossJ).ToWorldCoordinates();

            Player player = Main.LocalPlayer;
            if (NPC.AnyNPCs(ModContent.NPCType<LivingTrapBoss>()))
            {
                return true;

            }
            //int index = NPC.NewNPC(player.GetSource_FromAI(), (int)BossLocation.X, (int)BossLocation.Y, ModContent.NPCType<LivingTrapBoss>());
            if (Main.netMode == 0)
                NPC.NewNPC(player.GetSource_FromAI(), (int)BossLocation.X, (int)BossLocation.Y, ModContent.NPCType<LivingTrapBoss>());
            else
            {
                ModPacket myPacket = Mod.GetPacket();

                myPacket.Write((byte)UnveiledMystery.MessageType.SpawnNPCByPlayer);
                myPacket.Write((int)BossLocation.X);
                myPacket.Write((int)BossLocation.Y);
                myPacket.Write((int)ModContent.NPCType<LivingTrapBoss>());
                myPacket.Send();
            }


            return true;
        }

    }
}
