using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.DataStructures;
using UnveiledMystery.Buffs;
using UnveiledMystery.Enemies;

namespace UnveiledMystery.Items
{
    public class LivingTrapMinion_Item : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Minion Boss Pet");
            Tooltip.SetDefault("Summons a miniature Minion Boss to follow you");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ModContent.ProjectileType<LivingTrapMinion_Projectile>(), ModContent.BuffType<LivingTrapMinion_Buff>()); 

            Item.width = 28;
            Item.height = 20;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 5);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2); 

            return false;
        }
    }
}