using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace UnveiledMystery.Items
{
    internal class TurretCraftingStation_Item : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Turret Crafting Station");
            Tooltip.SetDefault("Allows you to craft turret with stone, hellstone, adamantite, titanium and Chlorophyte");
        }
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = false;
            Item.maxStack = 1;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange;
            Item.createTile = ModContent.TileType<Tiles.TurretCraftingStation_Tile>();
            Item.width = 10;
            Item.height = 24;
            Item.value = Item.buyPrice(gold: 10);

        }
    }
}
