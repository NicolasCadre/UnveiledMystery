using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using UnveiledMystery.Items;
using UnveiledMystery.Enemies.Boss;

namespace UnveiledMystery.Tiles
{
    public class LivingTrapArenaTrapDoor_Tile : ModTile
    {
        public override void SetStaticDefaults()
        {
            // Properties
            Main.tileFrameImportant[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileID.Sets.NotReallySolid[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            // Placement
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);
        }
        public override bool CanKillTile(int i, int j, ref bool blockDamaged)
        {
            return false;
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }


        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<LivingTrapArenaTrapDoor_Item>();
        }

        public override bool RightClick(int i, int j)
        {
            Point tileClickedPos;
            NPC boss = null;
            if (Main.tile[i-1, j].TileType == ModContent.TileType<LivingTrapArenaTrapDoor_Tile>())
                tileClickedPos = new Point(i-1, j);
            else
                tileClickedPos = new Point(i, j);
            Tile[] tiles = new Tile[] { Main.tile[tileClickedPos], Main.tile[tileClickedPos + new Point(1, 0)] };

            boss = SearchBoss();

            if(boss != null)
            {
                if (boss.active != true || boss == null)
                {
                    OpenClose(tiles);
                }
            }
            else
            {
                OpenClose(tiles);
            }

            return true;

        }

        private NPC SearchBoss()
        {
            NPC boss = null;
            foreach (NPC npc in Main.npc)
            {
                if(npc.type == ModContent.NPCType<LivingTrapBoss>() && npc.active == true && npc.life != 0)
                {
                    boss = npc;
                }
            }

            return boss;
        }

        private void OpenClose(Tile[] tiles)
        {
            if (tiles[0].TileFrameX == 0)
            {
                foreach (Tile tile in tiles)
                {
                    tile.TileFrameX += 36;
                    Main.tileSolid[Type] = false;
                }

            }
            else
            {
                foreach (Tile tile in tiles)
                {
                    tile.TileFrameX -= 36;
                    Main.tileSolid[Type] = true;
                }
            }
        }
    }
}