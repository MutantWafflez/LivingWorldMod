using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Custom.Utilities {

    /// <summary>
    /// Utilities class that deals with the Tile class and tiles in general.
    /// </summary>
    public static class TileUtilities {

        /// <summary>
        /// Checks and returns whether or not the given tile type at the given position can merge
        /// with the other tile at the position offset by the given offset. For example, passing the
        /// dirt tile type and its position and an offset of 0, 1 would check if dirt can merge with
        /// the tile right below it.
        /// </summary>
        /// <param name="tileType"> The tile type at the given position. </param>
        /// <param name="tilePosition"> The position of the tile in question. </param>
        /// <param name="otherTileOffset">
        /// The offset of the other tile in regards to the initial tile's position.
        /// </param>
        /// <returns> </returns>
        public static bool CanMergeWithTile(int tileType, Point tilePosition, Point otherTileOffset) {
            Tile otherTile = Framing.GetTileSafely(tilePosition + otherTileOffset);

            if (otherTile == null || !otherTile.IsActive) {
                return false;
            }

            return otherTile.type == tileType
                   || Main.tileMerge[tileType][otherTile.type]
                   || (otherTile.type == TileID.Dirt && (Main.tileMergeDirt[tileType] || TileID.Sets.ForcedDirtMerging[tileType]))
                   || (TileID.Sets.MergesWithClouds[tileType] && TileID.Sets.Clouds[otherTile.type])
                   || (TileID.Sets.OreMergesWithMud[tileType] && TileID.Sets.Mud[otherTile.type]);
        }
    }
}