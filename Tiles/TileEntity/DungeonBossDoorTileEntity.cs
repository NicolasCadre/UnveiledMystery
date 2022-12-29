using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Chat;
using Terraria.Localization;

namespace UnveiledMystery.Tiles.TileEntity
{
    public class DungeonBossDoorTileEntity : ModTileEntity
    {
        private int timerOpenDoor = 0;
        const int TIMEROPENDOORMAX = 300;
        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            //The MyTile class is shown later
            return tile.HasTile && tile.TileType == ModContent.TileType<DungeonBossDoorTile>();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                //Sync the entire multitile's area.  Modify "width" and "height" to the size of your multitile in tiles
                int width = 1;
                int height = 3;
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
            if (Main.tile[Position.X, Position.Y].TileFrameX == 54)
                DestroyBossDoor(Position.X, Position.Y);
        }

        public void DestroyBossDoor(int i, int j)
        {
            timerOpenDoor++;
            Player p = Main.LocalPlayer;
            if (timerOpenDoor == 1)
                p.GetModPlayer<CameraManager>().Shake(50, 119f);
            if (timerOpenDoor == 120)
                p.GetModPlayer<CameraManager>().Shake(100, 120f);
            if (timerOpenDoor == 240)
                p.GetModPlayer<CameraManager>().Shake(150, 60f);

            if (timerOpenDoor >= TIMEROPENDOORMAX)
                WorldGen.KillTile(i, j);
        }

    }
}
