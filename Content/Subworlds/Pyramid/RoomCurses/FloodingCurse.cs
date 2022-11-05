using LivingWorldMod.Common.ModTypes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.Subworlds.Pyramid.RoomCurses {
    /// <summary>
    /// Floods the bottom half of the room with water.
    /// </summary>
    public sealed class FloodingCurse : PyramidRoomCurse {
        public override void DoGenerationEffect(Rectangle roomRegion) {
            for (int y = roomRegion.Y + roomRegion.Height / 2; y <= roomRegion.Bottom; y++) {
                for (int x = roomRegion.X; x <= roomRegion.Right; x++) {
                    Tile tile = Main.tile[x, y];
                    tile.LiquidType = LiquidID.Water;
                    tile.LiquidAmount = byte.MaxValue;
                }
            }
        }
    }
}