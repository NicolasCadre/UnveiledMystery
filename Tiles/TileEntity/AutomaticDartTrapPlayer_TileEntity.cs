using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Linq;
using Terraria.Chat;
using UnveiledMystery.Projectiles;

namespace UnveiledMystery.Tiles.TileEntity
{
    public class AutomaticDartTrapPlayer_TileEntity : ModTileEntity
    {
        private int timer = 0; 
        private const int TILEACTIVATIONRADIUS = 1000;
        private int cadence = 60;
        private bool activated = false;
        private NPC targetNPC;
        private int[] tileTypes = new int[] { ModContent.TileType<AutomaticDartTrapPlayerTier1_Tile>(), ModContent.TileType<AutomaticDartTrapPlayerTier2_Tile>(), ModContent.TileType<AutomaticDartTrapPlayerTier3_Tile>(), ModContent.TileType<AutomaticDartTrapPlayerTier4_Tile>() };
        private int projectile = 0;
        private int damage = 0;
        private float speed = 1;
        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            //The MyTile class is shown later
            return tile.HasTile && tileTypes.Any(type => type == tile.TileType);
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                //Sync the entire multitile's area.  Modify "width" and "height" to the size of your multitile in tiles
                int width = 1;
                int height = 1;
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);

                //Sync the placement of the tile entity with other clients
                //The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }

            //ModTileEntity.Place() handles checking if the entity can be placed, then places it for you
            //Set "tileOrigin" to the same value you set TileObjectData.newTile.Origin to in the ModTile
            return Place(i ,j); ;
        }

        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }

        public override void Update()
        {
            if(projectile == 0 || damage == 0)
            {
                if (Main.tile[Position.X, Position.Y].TileType == tileTypes[0])
                {
                    projectile = ProjectileID.WoodenArrowFriendly;
                    damage = 30;
                    speed = 0.5f;
                    cadence = 120;
                }
                else if (Main.tile[Position.X, Position.Y].TileType == tileTypes[1])
                {
                    projectile = ProjectileID.ExplosiveBullet;
                    damage = 60;
                    speed = 1f;
                    cadence = 60;
                }
                else if (Main.tile[Position.X, Position.Y].TileType == tileTypes[2])
                {
                    projectile = ProjectileID.ExplosiveBullet;
                    damage = 60;
                    speed = 0.1f;
                    cadence = 30;
                }
                else if (Main.tile[Position.X, Position.Y].TileType == tileTypes[3])
                {
                    projectile = ModContent.ProjectileType<AutomaticPlayerDartTrapHomingProjectile>();
                    damage = 120;
                    speed = 1f;
                    cadence = 60;
                }
            }
            foreach (NPC npc in Main.npc)
            {
                if (Vector2.Distance(npc.Center, Position.ToWorldCoordinates()) <= TILEACTIVATIONRADIUS && npc.CanBeChasedBy())
                {
                    activated = true;
                    if(targetNPC == null || !targetNPC.active)
                        targetNPC = npc;
                    else if (Vector2.Distance(npc.Center, Position.ToWorldCoordinates()) < Vector2.Distance(targetNPC.Center, Position.ToWorldCoordinates()) || targetNPC == null)
                        targetNPC = npc;

                }
            }
            if (activated)
            {
                if (Vector2.Distance(targetNPC.Center, Position.ToWorldCoordinates()) > TILEACTIVATIONRADIUS || !targetNPC.active)
                {
                    targetNPC = null;
                    activated = false;
                }
            }
            if (activated && targetNPC != null)
            {
                timer++;
                if (timer > cadence)
                {
                    {
                        Vector2 direction = (targetNPC.Center - Position.ToWorldCoordinates()) * speed;

                        if(projectile != ModContent.ProjectileType<AutomaticPlayerDartTrapHomingProjectile>())
                            Projectile.NewProjectile(new EntitySource_TileEntity(this), Position.ToWorldCoordinates(), direction, projectile, damage, 5f, Main.myPlayer);
                        else
                            Projectile.NewProjectile(new EntitySource_TileEntity(this), Position.ToWorldCoordinates(), direction, projectile, damage, 5f, Main.myPlayer, targetNPC.whoAmI);

                    }
                    timer = 0;
                }
            }



            if (tileTypes.All(type => type != Main.tile[Position.X, Position.Y].TileType))
                Kill(Position.X, Position.Y);
        }

    }
}
