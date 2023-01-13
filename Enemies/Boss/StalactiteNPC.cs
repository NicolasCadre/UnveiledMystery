using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System.Linq;

namespace UnveiledMystery.Enemies.Boss
{
    public class StalactiteNPC : ModNPC
    {
        private bool doOnce = false;
        private NPC LivingTrapHead;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 16;
            NPC.height = 60;

            NPC.aiStyle = -1;
            NPC.dontTakeDamage = true;
            NPC.lifeMax = 1;
            NPC.friendly = true;

            NPC.value = Item.buyPrice(copper: 0);

            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.lavaImmune = true;

        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        //Shoot Projectile and kill themselve when the boss' hand hit a wall
        public void Fall()
        {
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, 0, 8, ModContent.ProjectileType<Projectiles.StalactiteProjectile>(), 100, 5f);

            NPC.life = 0;
            NPC.checkDead();
            NPC.active = false;
        }

        public override void AI()
        {
            if (!doOnce)
            {
                LivingTrapHead = Main.npc.FirstOrDefault(x => x.active && x.type == ModContent.NPCType<LivingTrapBoss>());
                doOnce = true;
            }
            if (LivingTrapHead == null || !LivingTrapHead.active || LivingTrapHead.life <= 0)
            {
                NPC.life = 0;
                NPC.checkDead();
                NPC.active = false;
            }
        }

    }
}
