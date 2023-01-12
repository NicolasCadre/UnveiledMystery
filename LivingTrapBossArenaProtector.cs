using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using IL.Terraria.ID;
using System.Linq;
using UnveiledMystery.Tiles;
using Terraria.ID;
using UnveiledMystery.Enemies.Boss;
using System;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader.IO;



namespace UnveiledMystery
{
    public class LivingTrapBossArenaProtector : ModSystem
    {
        public static List<Player> PlayersInArena = new List<Player>();
        public static Point bossSummonTilePosition;
        public static int[] ArenaCoordinates = new int[4];
        public static List<Tile> ArenaTiles = new List<Tile>();
        private bool check = false;
        private Player[] players;
        private int TimerCheckCoordinate = 0;
        private int TimerCheckPlayer = 0;
        public override void OnWorldLoad()
        {
            ArenaCoordinates = new int[4];
            for (int i = 0; i <= 3; i++)
                ArenaCoordinates[i] = 0;

        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("ArenaCoordinates"))
            {
                ArenaCoordinates = tag.GetIntArray("ArenaCoordinates");
                check = true;
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
                        if (tile.HasTile && tile.TileType != ModContent.TileType<LivingTrapSummonAltar_Tile>())
                        {
                            ArenaTiles.Add(tile);
                        }
                    }
                }
            }
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (ArenaCoordinates[0] !=0)
                tag["ArenaCoordinates"] = ArenaCoordinates;
        }


        public override void PostUpdateTime()
        {
            if (!check)
            {
                if (ArenaCoordinates.All(v => v == 0))
                {
                    TimerCheckCoordinate++;

                    if (TimerCheckCoordinate > 120)
                    {
                        if (bossSummonTilePosition.X == 0 && bossSummonTilePosition.Y == 0)
                        {
                            players = Main.player;
                            foreach (Player p in players)
                            {
                                for (int x = p.position.ToTileCoordinates().X - 100; x < p.position.ToTileCoordinates().X + 100; x++)
                                {
                                    for (int y = p.position.ToTileCoordinates().Y - 100; y < p.position.ToTileCoordinates().Y + 100; y++)
                                    {
                                        if (x >= 0 && y >= 0 && x <= Main.tile.Width - 1 && y <= Main.tile.Height - 1)
                                        {
                                            if (Main.tile[x, y].TileType == ModContent.TileType<LivingTrapSummonAltar_Tile>())
                                            {
                                                bossSummonTilePosition = new Point(x, y + 2);
                                                Point arenaLenght = new Vector2(3000, 432).ToTileCoordinates();
                                                Point ArenaPositionAndLenght = new Point(bossSummonTilePosition.X - arenaLenght.X, bossSummonTilePosition.Y - arenaLenght.Y - 1);

                                                ArenaCoordinates[0] = ArenaPositionAndLenght.X;
                                                ArenaCoordinates[1] = bossSummonTilePosition.X - 28;

                                                ArenaCoordinates[2] = ArenaPositionAndLenght.Y;
                                                ArenaCoordinates[3] = bossSummonTilePosition.Y - 1;
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        TimerCheckCoordinate = 0;

                    }
                }
                else
                {
                    if (bossSummonTilePosition.X > 0 && bossSummonTilePosition.Y > 0)
                    {
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
                                if (tile.HasTile && tile.TileType != ModContent.TileType<LivingTrapSummonAltar_Tile>())
                                {
                                    ArenaTiles.Add(tile);
                                }
                            }
                        }
                        check = true;
                    }
                }
            }
            else
            {
                TimerCheckPlayer++;
                if (TimerCheckPlayer >= 60)
                {

                    CheckArenaPlayer();
                    TimerCheckPlayer = 0;
                }
            }

            if (!ArenaCoordinates.All(v => v == 0))
            {
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

        public static void CheckArenaPlayer()
        {
            foreach (Player player in Main.player)
            {
                /*ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("ArenaCoordinates[0]" + ArenaCoordinates[0]), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("ArenaCoordinates[1]" + ArenaCoordinates[1]), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("ArenaCoordinates[2]" + ArenaCoordinates[2]), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("ArenaCoordinates[3]" + ArenaCoordinates[3]), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Main.player[0].position.ToTileCoordinates().X" + Main.player[0].position.ToTileCoordinates().X), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Main.player[0].position.ToTileCoordinates().Y" + Main.player[0].position.ToTileCoordinates().Y), Color.White);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("bossSummonTilePositionX = " + bossSummonTilePosition.X + "bossSummonTilePositionY = "+ bossSummonTilePosition.Y), Color.White);
                */
                

                Player currentPlayer = Main.player[player.whoAmI];
                if (ArenaCoordinates[0] != 0 && ArenaCoordinates[1] != 0 && ArenaCoordinates[2] != 0 && ArenaCoordinates[3] != 0)
                {
                    if (currentPlayer.position.ToTileCoordinates().X <= ArenaCoordinates[1] + 70 && currentPlayer.position.ToTileCoordinates().X >= ArenaCoordinates[0]-30 && currentPlayer.position.ToTileCoordinates().Y <= ArenaCoordinates[3] + 30 && currentPlayer.position.ToTileCoordinates().Y >= ArenaCoordinates[2]-100)
                    {
                        if (PlayersInArena.All(p => p.whoAmI != currentPlayer.whoAmI))
                        {
                            PlayersInArena.Add(Main.player[currentPlayer.whoAmI]);

                        }
                    }
                    else
                    {
                        foreach (Player p in PlayersInArena.ToList())
                        {
                            if (p.whoAmI == currentPlayer.whoAmI)
                            {
                                PlayersInArena.Remove(Main.player[currentPlayer.whoAmI]);
                            }
                        }
                    }
                }
            }
        }
    }
}
