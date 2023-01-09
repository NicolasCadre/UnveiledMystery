using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using UnveiledMystery.Buffs;

namespace UnveiledMystery.Biomes
{
    public class LivingTrapDungeonBIome : ModBiome
    {

        // Select Music
        public override int Music => MusicID.Graveyard;

        // Populate the Bestiary Filter
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh; // We have set the SceneEffectPriority to be BiomeLow for purpose of example, however default behavour is BiomeLow.
        // Use SetStaticDefaults to assign the display name
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Strange Temple");
        }

        // Calculate when the biome is active.
        public override bool IsBiomeActive(Player player)
        {
            // Limit the biome height to be underground in either rock layer or dirt layer
            return (player.ZoneRockLayerHeight || player.ZoneUnderworldHeight) &&
                // Check how many tiles of our biome are present, such that biome should be active
                ModContent.GetInstance<LivingTrapDungeonBiomeTileCount>().BlockCount >= 400 &&
                //Verify if the player is inside the dungeon (AKA if there are stone brick tile behind him)
                Framing.GetTileSafely(player.Center.ToTileCoordinates().X, player.Center.ToTileCoordinates().Y).WallType == WallID.GrayBrick ;
        }

        public override void OnInBiome(Player player)
        {
            player.AddBuff(ModContent.BuffType<LivingTrapCurse>(), 120);
        }
    }
}
public class LivingTrapDungeonBiomeTileCount : ModSystem
{
    public int BlockCount;

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    { 
        BlockCount = tileCounts[TileID.GrayBrick];
    }
}