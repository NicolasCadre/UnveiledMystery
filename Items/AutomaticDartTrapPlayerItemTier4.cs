using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace UnveiledMystery.Items
{
    internal class AutomaticDartTrapPlayerItemTier4 : ModItem
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
            Item.createTile = ModContent.TileType<Tiles.AutomaticDartTrapPlayerTileTier4>();
            Item.width = 16;
            Item.height = 16;
            Item.value = 0;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ChlorophyteBar, 5);
            recipe.AddIngredient(ModContent.ItemType<AutomaticDartTrapPlayerItemTier3>(), 1);
            recipe.AddTile(ModContent.TileType<Tiles.TurretCraftingStationTile>());
            recipe.Register();
        }
    }
}
