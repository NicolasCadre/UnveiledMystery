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
            VENOMING = 2,
            KNOCKBACK = 3,
            POISON = 4,
            ICHORING = 5,
            BURNING = 6,
            CONFUSING = 7,
            CURSEDINFERNO = 8,
            EVERYTHINGATONCE = 9

        }

        private RandomEffect effect;
        private RandomEffect previousEffect;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sword of Randomness");
            Tooltip.SetDefault("Each swing triggers one of nine random effect."); 

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 80; 
            Item.height = 80; 

            Item.useStyle = ItemUseStyleID.Swing; 
            Item.useTime = 35; 
            Item.useAnimation = 35; 
            Item.autoReuse = true; 

            Item.DamageType = DamageClass.Melee; 
            Item.damage = 30; 
            Item.knockBack = 2; 
            Item.crit = 0; 

            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1; 
        }

        public override bool? UseItem(Player player)
        {

            effect = (RandomEffect)Main.rand.Next(1, 10);
            while (effect == previousEffect)
                effect = (RandomEffect)Main.rand.Next(1, 10);
            previousEffect = effect;
            Rectangle textPos = new Rectangle((int)player.position.X, (int)player.position.Y - 20, player.width, player.height);
            switch (effect)
            {
                case RandomEffect.ONFIRE:
                    Item.knockBack = 2;
                    SoundEngine.PlaySound(SoundID.Item20);
                    CombatText.NewText(textPos, Color.OrangeRed, 1, false, false);
                    break;
                case RandomEffect.EVERYTHINGATONCE:
                    Item.knockBack = player.GetWeaponKnockback(Item) + 100;
                    player.GetModPlayer<CameraManager>().Shake(25, 20f);
                    CombatText.NewText(textPos, Color.HotPink, 9, true, false);
                    SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Items/RandomSwordSpecial"));
                    break;
                case RandomEffect.KNOCKBACK:
                    Item.knockBack = player.GetWeaponKnockback(Item) + 100;
                    CombatText.NewText(textPos, Color.White, 3, false, false);
                    SoundEngine.PlaySound(new SoundStyle("UnveiledMystery/Sounds/Items/RandomSwordKnockback"));
                    break;
                case RandomEffect.POISON:
                    Item.knockBack = 2;
                    CombatText.NewText(textPos, Color.LimeGreen, 4, false, false);
                    SoundEngine.PlaySound(SoundID.Item44);
                    break;
                case RandomEffect.ICHORING:
                    Item.knockBack = 2;
                    CombatText.NewText(textPos, Color.Yellow, 5, false, false);
                    SoundEngine.PlaySound(SoundID.NPCDeath19);
                    break;
                case RandomEffect.BURNING:
                    Item.knockBack = 2;
                    CombatText.NewText(textPos, Color.OrangeRed, 6, false, false);
                    SoundEngine.PlaySound(SoundID.Item34);
                    break;
                case RandomEffect.CONFUSING:
                    Item.knockBack = 2;
                    CombatText.NewText(textPos, Color.Salmon, 7, false, false);
                    SoundEngine.PlaySound(SoundID.Item9);
                    break;
                case RandomEffect.CURSEDINFERNO:
                    Item.knockBack = 2;
                    CombatText.NewText(textPos, Color.LightGreen, 8, false, false);
                    SoundEngine.PlaySound(SoundID.Item20 with
                    {
                        Pitch = -1f
                    });
                    break;
                case RandomEffect.VENOMING:
                    Item.knockBack = 2;
                    CombatText.NewText(textPos, Color.MediumPurple, 2, false, false);
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
