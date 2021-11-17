using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace LivingWorldMod.Custom.Utilities {
    /// <summary>
    /// Utilities class that deals with the Tile class and tiles in general.
    /// </summary>
    public static class TileUtils {
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

            if (otherTile is not { IsActive: true }) {
                return false;
            }

            return otherTile.type == tileType
                   || Main.tileMerge[tileType][otherTile.type]
                   || otherTile.type == TileID.Dirt && (Main.tileMergeDirt[tileType] || TileID.Sets.ForcedDirtMerging[tileType])
                   || TileID.Sets.MergesWithClouds[tileType] && TileID.Sets.Clouds[otherTile.type]
                   || TileID.Sets.OreMergesWithMud[tileType] && TileID.Sets.Mud[otherTile.type];
        }

        /// <summary>
        /// Returns the top left coordinate of the passed in tile which should be a type of multi-tile.
        /// </summary>
        /// <param name="tile"> A tile within the multi-tile. </param>
        /// <param name="x"> The x coordinate of the specified tile. </param>
        /// <param name="y"> The y coordinate of the specified tile. </param>
        public static Point16 GetTopLeftOfMultiTile(Tile tile, int x, int y) => new Point16(x - tile.frameX / 18, y - tile.frameY / 18);

        /// <summary>
        /// Returns the top left coordinate of the passed in tile which should be a type of multi-tile. This overload takes into account multi-
        /// styled multi-tiles.
        /// </summary>
        /// <param name="tile"> A tile within the multi-tile. </param>
        /// <param name="x"> The x coordinate of the specified tile. </param>
        /// <param name="y"> The y coordinate of the specified tile. </param>
        /// <param name="multiTileWidth">
        /// The width of the full multi-tile, in tiles.
        /// Assumes multi-tile sprite uses 16x16 for each tile frame, with 2 pixels worth of padding.
        /// </param>
        public static Point16 GetTopLeftOfMultiTile(Tile tile, int x, int y, int multiTileWidth) {
            int pixelWidthOfFullTile = multiTileWidth * 18;
            //Calculate proper displacement
            int frameXDisplacement = tile.frameX >= pixelWidthOfFullTile ? (int)(tile.frameX / (float)pixelWidthOfFullTile) * pixelWidthOfFullTile : 0;

            return new Point16(x - (tile.frameX - frameXDisplacement) / 18, y - tile.frameY / 18);
        }
    }
}