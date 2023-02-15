using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using UnveiledMystery.Enemies.Boss;

namespace UnveiledMystery.Projectiles
{
    internal class StalactiteProjectile : ModProjectile
    {
        private bool doOnce = false;
        private LivingTrapBoss Boss;
        private NPC BossNPC;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stalactites");
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 60;
            Projectile.aiStyle = 0;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
            Projectile.light = 0f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            //Link to the Boss
            if (!doOnce)
            {
                BossNPC = Main.npc.FirstOrDefault(x => x.active && x.type == ModContent.NPCType<LivingTrapBoss>());
                Boss = (LivingTrapBoss)BossNPC.ModNPC;
                doOnce = true;
            }

            //Makes projectiles more aggressive if the boss is enraged
            if (Boss.Enraged)
            {
                Projectile.tileCollide = false;
                Projectile.timeLeft = 1000;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i <= 10; i++)
                Dust.NewDust(Projectile.position + new Vector2(0, 20), 16, 10, DustID.Stone);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            return true;

        }
    }
}
