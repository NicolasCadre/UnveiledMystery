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
using UnveiledMystery.Enemies;

namespace UnveiledMystery
{
    public class MirrorRoomCheck : ModSystem
    {
        public static List<Player> PlayersCurrentlyInRoom = new List<Player>();
        private Point CheckTilePosition;
        public static int[] RoomCoordinate = new int[4]; // 0 = left X, 1 = right X, 2 = Top Y, 3 = Bottom Y
        private Player[] players;
        private int TimerCheckCoordinate = 0;
        private int TimerCheckPlayer = 0;
        public static List<NPC> Ghosts = new List<NPC>();

        public override void OnWorldLoad()
        {
            RoomCoordinate = new int[4];

        }
        /*public override void SaveWorldData(TagCompound tag)
        {
            tag["MirrorRoomCoordinates"] = RoomCoordinate;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("MirrorRoomCoordinates"))
                RoomCoordinate = tag.GetIntArray("MirrorRoomCoordinates");
        }*/
        public override void PostUpdateTime()
        {
            if (RoomCoordinate.All(v => v == 0))
            {
                TimerCheckCoordinate++;

                if (TimerCheckCoordinate > 120)
                {
                    if (CheckTilePosition.X == 0 && CheckTilePosition.Y == 0)
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
                                        if (Main.tile[x, y].TileType == ModContent.TileType<TileCheckMirrorRoom>())
                                        {
                                            CheckTilePosition = new Point(x, y);
                                            Point RoomLenght = new Vector2(1000, 450).ToTileCoordinates();
                                            Point RoomPositionAndLenght = new Point(CheckTilePosition.X - RoomLenght.X, CheckTilePosition.Y - RoomLenght.Y);

                                            RoomCoordinate[0] = RoomPositionAndLenght.X;
                                            RoomCoordinate[1] = CheckTilePosition.X;

                                            RoomCoordinate[2] = RoomPositionAndLenght.Y;
                                            RoomCoordinate[3] = CheckTilePosition.Y;
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
                TimerCheckPlayer++;
                if(TimerCheckPlayer >=60)
                {
                    /*ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("RoomCoordinate[0]" + RoomCoordinate[0]), Color.White);
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("RoomCoordinate[1]" + RoomCoordinate[1]), Color.White);
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("RoomCoordinate[2]" + RoomCoordinate[2]), Color.White);
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("RoomCoordinate[3]" + RoomCoordinate[3]), Color.White);
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Main.player[0].position.ToTileCoordinates().X" + Main.player[0].position.ToTileCoordinates().X), Color.White);
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Main.player[0].position.ToTileCoordinates().Y" + Main.player[0].position.ToTileCoordinates().Y), Color.White);
                    */
                    CheckPlayersInRoom();
                    TimerCheckPlayer = 0;
                }

            }
        }

        public static void CheckPlayersInRoom()
        {
            foreach (Player player in Main.player)
            {
                Player currentPlayer = Main.player[player.whoAmI];
                if (RoomCoordinate[0] != 0 && RoomCoordinate[1] != 0 && RoomCoordinate[2] != 0 && RoomCoordinate[3] != 0)
                {
                    // If a new player enters the room, spawn a ghost
                    if (currentPlayer.position.ToTileCoordinates().X <= RoomCoordinate[1] && currentPlayer.position.ToTileCoordinates().X >= RoomCoordinate[0] && currentPlayer.position.ToTileCoordinates().Y <= RoomCoordinate[3] && currentPlayer.position.ToTileCoordinates().Y >= RoomCoordinate[2])
                    {
                        if (PlayersCurrentlyInRoom.All(p => p.whoAmI != currentPlayer.whoAmI))
                        {
                            PlayersCurrentlyInRoom.Add(Main.player[currentPlayer.whoAmI]);
                            int newGhost = NPC.NewNPC(player.GetSource_FromAI(), (int)player.position.X, (int)player.position.Y - 200, ModContent.NPCType<MirrorGhost>());
                            MirrorGhost GhostScript = (MirrorGhost)Main.npc[newGhost].ModNPC;
                            GhostScript.TargetPlayer = currentPlayer;
                            Ghosts.Add(Main.npc[newGhost]);
                            for (int i = 0; i <= 20; i++)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(0.5f, 1f);
                                Dust.NewDust(Main.npc[newGhost].Center, 0, 0, DustID.BlueTorch, speed.X, speed.Y);
                            }

                        }
                    }
                    // Kills the ghost if the player leaves the room
                    else
                    {

                        foreach (Player p in PlayersCurrentlyInRoom.ToList())
                        {
                            if (p.whoAmI == currentPlayer.whoAmI)
                            {

                                foreach (NPC ghost in Ghosts.ToList())
                                {
                                    MirrorGhost GhostScript = (MirrorGhost)ghost.ModNPC;
                                    if(GhostScript.TargetPlayer == currentPlayer)
                                    {
                                        for(int i =0; i <= 20; i++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2Circular(0.5f, 1f);
                                            Dust.NewDust(ghost.Center, 0, 0, 59, speed.X, speed.Y);
                                        }
                                        ghost.life = 0;
                                        ghost.checkDead();
                                        ghost.active = false;
                                        Ghosts.Remove(ghost);
                                    }
                                }
                                PlayersCurrentlyInRoom.Remove(Main.player[currentPlayer.whoAmI]);
                            }
                        }
                    }
                }
            }
        }
    }
}
