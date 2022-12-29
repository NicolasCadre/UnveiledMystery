using Terraria.ID;
using Terraria.ModLoader;

namespace UnveiledMystery.Items
{
    internal class InvisibleTileItem : ModItem
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
            Item.createTile = ModContent.TileType<Tiles.InvisibleTile>();
            Item.width = 16;
            Item.height = 16;
            Item.value = 0;
        }
    }
}
