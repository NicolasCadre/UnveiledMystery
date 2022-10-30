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



            for(int number = 0; number <= 50; number++)
            {
                int i = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                int j = WorldGen.genRand.Next(0, Main.maxTilesY - 800);
                StructureHelper.Generator.GenerateStructure("Structure/BaseStructure", new Point16(i, j), mod);
            }

            for (int x = 0; x < Main.tile.Width; x++)
            {
                for (int y = 0; y < Main.tile.Height; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.TileType == ModContent.TileType<RoomChoserTile0>())
                    {
                        StructureHelper.Generator.GenerateMultistructureRandom("Structure/Room0", new Point16(x, y), mod);
                        tile.HasTile = false;
                    }
                    else if (tile.TileType == ModContent.TileType<RoomChoserTile1>())
                    {
                        StructureHelper.Generator.GenerateMultistructureRandom("Structure/Room1", new Point16(x, y), mod);
                        tile.HasTile = false;
                    }

                }
            }
        }

    }
}
