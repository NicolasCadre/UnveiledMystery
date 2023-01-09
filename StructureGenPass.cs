using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.IO;
using System.Linq;
using Terraria.DataStructures;
using UnveiledMystery.Tiles;
using System;

namespace UnveiledMystery
{
    internal class StructureGenPass : GenPass
    {
        private int[] tileTypesToNotTouch = new int[] { TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick, TileID.SandstoneBrick, TileID.ObsidianBrick, TileID.HellstoneBrick, TileID.RichMahogany, TileID.LihzahrdBrick, TileID.HoneyBlock, TileID.Hive };
        private const int DUNGEONSIZE_X = 256;
        private const int DUNGEONSIZE_Y = 192;
        public StructureGenPass(string name, float weight) : base(name, weight) { }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Add a strange ruin";
            Mod mod = ModLoader.GetMod("UnveiledMystery");

            //Generate the structure's skeleton
            int i = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
            int j = WorldGen.genRand.Next((int)WorldGen.worldSurface+200, Main.maxTilesY - 300);

            //Prevent the Structure to clash with other vanilla important structures
            bool isSafe = IsNotDestroyingOtherStructure(i, j);
            while(!isSafe)
            {
                if (!isSafe)
                {
                    i = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                    j = WorldGen.genRand.Next((int)WorldGen.worldSurface + 200, Main.maxTilesY - 300);
                    isSafe = IsNotDestroyingOtherStructure(i, j);
                }
                if (isSafe)
                    StructureHelper.Generator.GenerateStructure("Structure/DungeonMain", new Point16(i, j), mod);

            }


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

        private bool IsNotDestroyingOtherStructure(int i, int j)
        {
            if (i + DUNGEONSIZE_X > Main.tile.Width || j + DUNGEONSIZE_Y > Main.tile.Height)
                return false;
            for(int x = i; x <= i + DUNGEONSIZE_X; x++)
            {
                for (int y = j; y <= j + DUNGEONSIZE_Y; y++)
                {
                    if (tileTypesToNotTouch.Any(t => t == Main.tile[x, y].TileType))
                        return false;
                }
            }
            return true;
        }

    }
}
