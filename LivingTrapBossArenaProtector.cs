using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using IL.Terraria.ID;
using System.Linq;
using UnveiledMystery.Tiles;

namespace UnveiledMystery
{
    public class LivingTrapBossArenaProtector : ModSystem
    {
        private Point bossSummonTilePosition;
        public static int[] ArenaCoordinates = new int[4];
        public static List<Tile> ArenaTiles = new List<Tile>();
        private bool doOnce = false;
        public override void OnWorldLoad()
        {
            doOnce = false;

        }

        public override void PostUpdateTime()
        {
            if (!doOnce)
            {
                for (int x = 0; x < Main.tile.Width; x++)
                {
                    for (int y = 0; y < Main.tile.Height; y++)
                    {
                        if (Main.tile[x, y].TileType == ModContent.TileType<BossSummonAltarTile>())
                            bossSummonTilePosition = new Point(x, y + 2);
                    }
                }
                Point arenaLenght = new Vector2(3000, 432).ToTileCoordinates();
                Point ArenaPositionAndLenght = new Point(bossSummonTilePosition.X - arenaLenght.X, bossSummonTilePosition.Y - arenaLenght.Y - 1);


                ArenaCoordinates[0] = ArenaPositionAndLenght.X;
                ArenaCoordinates[1] = bossSummonTilePosition.X - 28;

                ArenaCoordinates[2] = ArenaPositionAndLenght.Y;
                ArenaCoordinates[3] = bossSummonTilePosition.Y - 1;

                for (int x = ArenaCoordinates[0]; x <= ArenaCoordinates[1]; x++)
                {
                    for (int y = ArenaCoordinates[2]; y < ArenaCoordinates[3]; y++)
                    {
                        Tile tile = Main.tile[x, y];
                        if (!tile.HasTile)
                        {
                            tile.HasTile = true;
                            tile.TileType = 38;
                        }
                        if (tile.HasTile && tile.TileType != ModContent.TileType<BossSummonAltarTile>())
                        {
                            ArenaTiles.Add(tile);
                        }
                    }
                }
                doOnce = true;
            }
            // Prevent items dropped by players in the western part of the arena to be innaccessible after the wall has been closed of
            foreach (Item item in Main.item)
            {
                if (item.position.ToTileCoordinates().X <= ArenaCoordinates[1] && item.position.ToTileCoordinates().X >= ArenaCoordinates[0])
                {
                    if (item.position.ToTileCoordinates().Y <= ArenaCoordinates[3] && item.position.ToTileCoordinates().Y >= ArenaCoordinates[2])
                    {
                        if (Main.tile[item.position.ToTileCoordinates()].HasTile)
                            item.position = bossSummonTilePosition.ToWorldCoordinates() - new Vector2(0, 50);
                    }
                }
            }
        }
    }
}
