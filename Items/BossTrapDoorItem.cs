using Terraria.ID;
using Terraria.ModLoader;

namespace UnveiledMystery.Items
{
    internal class BossTrapDoorItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = false;
            Item.maxStack = 1;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.BossTrapDoorTile>();
            Item.width = 10;
            Item.height = 24;
            Item.value = 500;
        }
    }
}
