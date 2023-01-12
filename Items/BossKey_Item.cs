using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria;
namespace UnveiledMystery.Items
{
    public class BossKey_Item : ModItem
    {

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.None;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = false;
            Item.maxStack = 100;
            Item.consumable = false;
            Item.width = 30;
            Item.height = 30;
            Item.value = 0;

        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Strange Door Key");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<BossKeyPart_Item>(), 3);
            recipe.Register();
        }

    }
}
