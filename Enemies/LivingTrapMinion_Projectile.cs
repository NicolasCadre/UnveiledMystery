using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ModLoader;

namespace UnveiledMystery.Enemies
{
    public class LivingTrapMinion_Projectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Minion Boss Pet");

            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.CompanionCube); 


            // Projectile.aiStyle = -1; 
        }


       /* public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            CheckActive(player);


        }

        private void CheckActive(Player player)
        {
            if (!player.dead && player.HasBuff(ModContent.BuffType<LivingTrapMinion_Buff>()))
            {
                Projectile.timeLeft = 2;
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {

            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }*/
    }
}
