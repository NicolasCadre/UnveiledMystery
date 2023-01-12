using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
namespace UnveiledMystery.Tiles.TileEntity
{
    public class AutomaticDartTrapRain_TileEntity : ModTileEntity
    {
        private int timer = 0;

        private const int TILEACTIVATIONRADIUS = 1000;
        private bool activated = false;
        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            //The MyTile class is shown later
            return tile.HasTile && tile.TileType == ModContent.TileType<AutomaticDartTrapRain_Tile>();
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
            if(Main.netMode != NetmodeID.SinglePlayer)
            {
                if (Main.player.Any(p => Vector2.Distance(Position.ToWorldCoordinates(), p.position) <= TILEACTIVATIONRADIUS))
                    activated = true;
                else
                    activated = false;
            }
            else
            {
                if (Vector2.Distance(Position.ToWorldCoordinates(), Main.LocalPlayer.position) <= TILEACTIVATIONRADIUS)
                    activated = true;
                else
                    activated = false;
            }


            timer++;
            if (timer > 120)
            {
                if (activated)
                    Projectile.NewProjectile(new EntitySource_Misc(""), Position.ToWorldCoordinates(), new Vector2(0, 7.5f), ProjectileID.PoisonDart, 30, 5f);
                timer = 0;
            }



            if (Main.tile[Position.X, Position.Y].TileType != ModContent.TileType<AutomaticDartTrapRain_Tile>())
                Kill(Position.X, Position.Y);
        }

    }
}
