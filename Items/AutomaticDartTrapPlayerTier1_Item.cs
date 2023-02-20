using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace UnveiledMystery.Items
{
    internal class AutomaticDartTrapPlayerTier1_Item : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stone Dart Turret");
            Tooltip.SetDefault("Shoots darts dealing 50 damages every 2 seconds.");
        }
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = false;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 100;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.AutomaticDartTrapPlayerTier1_Tile>();
            Item.width = 16;
            Item.height = 16;
            Item.value = 0;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.StoneBlock, 10);
            recipe.AddTile(ModContent.TileType<Tiles.TurretCraftingStation_Tile>());
            recipe.Register();
        }
    }
}
