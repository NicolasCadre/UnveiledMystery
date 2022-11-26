using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.Audio;
using UnveiledMystery.Dusts;

namespace UnveiledMystery.Items.Weapons
{
    public class RandomSword : ModItem
    {
        enum RandomEffect
        {
            ONFIRE = 1,
            EVERYTHINGATONCE = 2,
            KNOCKBACK = 3,
            POISON = 4,
            ICHORING = 5,
            BURNING = 6,
            CONFUSING = 7,
            CURSEDINFERNO = 8,
            VENOMING = 9
        }

        private RandomEffect effect;
        private RandomEffect previousEffect;
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("This is a Random sword."); // The (English) text shown below your weapon's name.

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 40; // The item texture's width.
            Item.height = 40; // The item texture's height.

            Item.useStyle = ItemUseStyleID.Swing; // The useStyle of the Item.
            Item.useTime = 40; // The time span of using the weapon. Remember in terraria, 60 frames is a second.
            Item.useAnimation = 40; // The time span of the using animation of the weapon, suggest setting it the same as useTime.
            Item.autoReuse = true; // Whether the weapon can be used more than once automatically by holding the use button.

            Item.DamageType = DamageClass.Melee; // Whether your item is part of the melee class.
            Item.damage = 1; // The damage your item deals.
            Item.knockBack = 2; // The force of knockback of the weapon. Maximum is 20
            Item.crit = 0; // The critical strike chance the weapon has. The player, by default, has a 4% critical strike chance.

            Item.value = Item.buyPrice(silver: 1); // The value of the weapon in copper coins.
            Item.UseSound = SoundID.Item1; // The sound when the weapon is being used.
        }

        public override bool? UseItem(Player player)
        {

            effect = (RandomEffect)Main.rand.Next(1, 10);
            while (effect == previousEffect)
                effect = (RandomEffect)Main.rand.Next(1, 10);
            switch (effect)
            {
                case RandomEffect.ONFIRE:
                    Item.knockBack = 2;
                    SoundEngine.PlaySound(SoundID.Item20);
                    break;
                case RandomEffect.EVERYTHINGATONCE:
                    Item.knockBack = player.GetWeaponKnockback(Item) + 100;
                    player.GetModPlayer<CameraManager>().Shake(25, 20f);
                    SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Items/RandomSwordSpecial"));
                    break;
                case RandomEffect.KNOCKBACK:
                    Item.knockBack = player.GetWeaponKnockback(Item) + 100;
                    SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Items/RandomSwordKnockback"));
                    break;
                case RandomEffect.POISON:
                    Item.knockBack = 2;
                    SoundEngine.PlaySound(SoundID.Item44);
                    break;
                case RandomEffect.ICHORING:
                    Item.knockBack = 2;
                    SoundEngine.PlaySound(SoundID.NPCDeath19);
                    break;
                case RandomEffect.BURNING:
                    Item.knockBack = 2;
                    SoundEngine.PlaySound(SoundID.Item34);
                    break;
                case RandomEffect.CONFUSING:
                    Item.knockBack = 2;
                    SoundEngine.PlaySound(SoundID.Item9);
                    break;
                case RandomEffect.CURSEDINFERNO:
                    Item.knockBack = 2;
                    SoundEngine.PlaySound(SoundID.Item20 with
                    {
                        Pitch = -1f
                    });
                    break;
                case RandomEffect.VENOMING:
                    Item.knockBack = 2;
                    SoundEngine.PlaySound(SoundID.Item43);
                    break;
            }
            return true;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            int DustTypeID = 0;
            switch (effect)
            {
                case RandomEffect.ONFIRE:
                    DustTypeID = 6;
                    break;
                case RandomEffect.POISON:
                    DustTypeID = 163;
                    break;
                case RandomEffect.ICHORING:
                    DustTypeID = 169;
                    break;
                case RandomEffect.BURNING:
                    DustTypeID = 55;
                    break;
                case RandomEffect.CONFUSING:
                    DustTypeID = 97;
                    break;
                case RandomEffect.CURSEDINFERNO:
                    DustTypeID = 75;
                    break;
                case RandomEffect.VENOMING:
                    DustTypeID = 171;
                    break;
                case RandomEffect.KNOCKBACK:
                    DustTypeID = ModContent.DustType<RandomSwordKnockbackDust>();
                    break;
                case RandomEffect.EVERYTHINGATONCE:
                    DustTypeID = ModContent.DustType<RandomSwordSpecialDust>();
                    break;
                default:
                    DustTypeID = 0;
                    break;

            }

            if (DustTypeID != 0)
            {
                Vector2 Pos = new Vector2(hitbox.X, hitbox.Y);
                int dindex = Dust.NewDust(Pos, hitbox.Width / 2, hitbox.Height / 2, DustTypeID, 0, 0);
                Dust d = Main.dust[dindex];
                d.noGravity = false;
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
        {
            switch (effect)
            {
                case RandomEffect.ONFIRE:
                    target.AddBuff(BuffID.OnFire, 240);
                    break;
                case RandomEffect.POISON:
                    target.AddBuff(BuffID.Poisoned, 320);
                    break;
                case RandomEffect.ICHORING:
                    target.AddBuff(BuffID.Ichor, 320);
                    break;
                case RandomEffect.BURNING:
                    target.AddBuff(BuffID.Burning, 120);
                    break;
                case RandomEffect.CONFUSING:
                    target.AddBuff(BuffID.Confused, 240);
                    break;
                case RandomEffect.CURSEDINFERNO:
                    target.AddBuff(BuffID.CursedInferno, 240);
                    break;
                case RandomEffect.VENOMING:
                    target.AddBuff(BuffID.Venom, 120);
                    break;
                case RandomEffect.EVERYTHINGATONCE:
                    target.AddBuff(BuffID.OnFire, 240);
                    target.AddBuff(BuffID.Poisoned, 320);
                    target.AddBuff(BuffID.Burning, 120);
                    target.AddBuff(BuffID.Confused, 240);
                    target.AddBuff(BuffID.CursedInferno, 240);
                    target.AddBuff(BuffID.Venom, 120);
                    break;
            }
        }
    }



}
