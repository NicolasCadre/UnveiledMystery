using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.IO;
using System;
using Terraria.DataStructures;
using UnveiledMystery.Tiles;

namespace UnveiledMystery
{
    internal class StructureGenPass : GenPass
    {
        public StructureGenPass(string name, float weight) : base(name, weight) { }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Add a strange ruin";
            Mod mod = ModLoader.GetMod("UnveiledMystery");

            //Generate the structure's skeleton
            int i = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
            int j = WorldGen.genRand.Next((int)WorldGen.worldSurface+200, Main.maxTilesY - 300);
            StructureHelper.Generator.GenerateStructure("Structure/DungeonMain", new Point16(i, j), mod);

            //Generate the structure rooms
            for (int x = 0; x < Main.tile.Width; x++)
            {
                for (int y = 0; y < Main.tile.Height; y++)
                {
                    Tile tile = Main.tile[x, y];

                    if (tile.TileType == ModContent.TileType<RoomChoserTile>())
                    {
                        int frame = tile.TileFrameX / 18;
                        switch (frame)
                        {
                            case 0:
                                StructureHelper.Generator.GenerateMultistructureRandom("Structure/DungeonRoom0", new Point16(x, y), mod);
                                break;
                            case 1:
                                StructureHelper.Generator.GenerateStructure("Structure/DungeonRoom1", new Point16(x, y), mod);
                                break;
                            case 2:
                                StructureHelper.Generator.GenerateStructure("Structure/DungeonRoom2", new Point16(x, y), mod);
                                break;
                            case 3:
                                StructureHelper.Generator.GenerateStructure("Structure/DungeonRoom3", new Point16(x, y), mod);
                                break;

                        }
                    }

                }
            }
        }

    }
}
