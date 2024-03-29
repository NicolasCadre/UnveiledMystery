﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace UnveiledMystery.Items
{
    internal class AutomaticDartTrapPlayerTier4_Item : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Chlorophyte Turret");
            Tooltip.SetDefault("Shoots homing bullets dealing 120 damages every second.");
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = false;
            Item.maxStack = 100;
            Item.consumable = true;
            Item.rare = ItemRarityID.Lime;
            Item.createTile = ModContent.TileType<Tiles.AutomaticDartTrapPlayerTier4_Tile>();
            Item.width = 16;
            Item.height = 16;
            Item.value = 0;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ChlorophyteBar, 5);
            recipe.AddIngredient(ModContent.ItemType<AutomaticDartTrapPlayerTier3_Item>(), 1);
            recipe.AddTile(ModContent.TileType<Tiles.TurretCraftingStation_Tile>());
            recipe.Register();
        }
    }
}
