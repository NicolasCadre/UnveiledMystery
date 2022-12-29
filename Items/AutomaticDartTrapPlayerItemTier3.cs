using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace UnveiledMystery.Items
{
    internal class AutomaticDartTrapPlayerItemTier3 : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = false;
            Item.maxStack = 100;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.AutomaticDartTrapPlayerTileTier3>();
            Item.width = 16;
            Item.height = 16;
            Item.value = 0;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.TitaniumBar, 5);
            recipe.AddIngredient(ModContent.ItemType<AutomaticDartTrapPlayerItemTier2>(), 1);
            recipe.AddTile(ModContent.TileType<Tiles.TurretCraftingStationTile>());
            recipe.Register();
            recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.AdamantiteBar, 5);
            recipe.AddIngredient(ModContent.ItemType<AutomaticDartTrapPlayerItemTier2>(), 1);
            recipe.AddTile(ModContent.TileType<Tiles.TurretCraftingStationTile>());
            recipe.Register();
        }
    }
}
