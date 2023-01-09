using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace UnveiledMystery.Buffs
{
    public class LivingTrapCurse : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("LivingTrapCurse"); // Buff display name
            Description.SetDefault("Prevent you for mining, building and using Rod of Discord"); // Buff description
            Main.debuff[Type] = true;  // Is it a debuff?
            Main.pvpBuff[Type] = false; // Players can give other players buffs, which are listed as pvpBuff
            Main.buffNoSave[Type] = false; // Causes this buff not to persist when exiting and rejoining the world
            BuffID.Sets.LongerExpertDebuff[Type] = false; // If this buff is a debuff, setting this to true will make this buff last twice as long on players in expert mode
        }

        // Allows you to make this buff give certain effects to the given player
        public override void Update(Player player, ref int buffIndex)
        {
            player.noBuilding = true;
        }
    }

    public class RodOfDiscordPreventer : GlobalItem
    {
        public override bool CanUseItem(Item item, Player player)
        {
            if(item.type == ItemID.RodofDiscord)
            {
                if (player.buffType.Any(buff => buff == ModContent.BuffType<LivingTrapCurse>()))
                    return false;
                else
                    return true;
            }
            return true;

        }
    }
}