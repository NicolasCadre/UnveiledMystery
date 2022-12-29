using Terraria.ModLoader;
using Terraria.WorldBuilding;
using System.Collections.Generic;
using System;



namespace UnveiledMystery
{
    internal class WorldSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int StructureIndex = tasks.FindIndex(t => t.Name.Equals("Micro Biomes"));
            if (StructureIndex != -1)
            {
                tasks.Insert(StructureIndex + 1, new StructureGenPass("StructureGenPass", 320f));
            }
        }
    }
}