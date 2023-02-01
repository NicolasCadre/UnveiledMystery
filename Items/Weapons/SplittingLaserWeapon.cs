using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.Audio;
using UnveiledMystery.Dusts;
using UnveiledMystery.Projectiles;

namespace UnveiledMystery.Items.Weapons
{
    public class SplittingLaserWeapon : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Shoot a laser beam splitting itself perpendicularly");
        }

        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.DamageType = DamageClass.Magic;
            Item.width = 62;
            Item.height = 32;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0;
            Item.value = 10000;
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SplittingLaserProjectile>();
            Item.shootSpeed = 7;
            Item.crit = 32;
            Item.mana = 10;



        }
    }
}
