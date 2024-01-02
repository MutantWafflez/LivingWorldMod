using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Drawing;
using Terraria.ObjectData;

namespace LivingWorldMod.Custom.Utilities;

/// <summary>
/// Utilities class that deals with the Tile class and tiles in general.
/// </summary>
public static partial class Utilities {
    public enum CornerType : byte {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// Allows for calling of the private method "AddSpecialPoint(int, int, int)" in <see cref="TileDrawing"/>.
    /// </summary>
    public static readonly Delegate AddSpecialPoint = AddSpecialPoint = typeof(TileDrawing).GetMethod("AddSpecialPoint", BindingFlags.Instance | BindingFlags.NonPublic)!.CreateDelegate<Action<int, int, int>>(Main.instance.TilesRenderer);

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

        if (otherTile is not { HasTile: true }) {
            return false;
        }

        return otherTile.TileType == tileType
               || Main.tileMerge[tileType][otherTile.TileType]
               || otherTile.TileType == TileID.Dirt && (Main.tileMergeDirt[tileType] || TileID.Sets.ForcedDirtMerging[tileType])
               || TileID.Sets.MergesWithClouds[tileType] && TileID.Sets.Clouds[otherTile.TileType]
               || TileID.Sets.OreMergesWithMud[tileType] && TileID.Sets.Mud[otherTile.TileType];
    }

    /// <summary>
    /// Returns the top left coordinate of the passed in tile which should be a type of multi-tile. This overload takes into
    /// account multi-styled multi-tiles.
    /// </summary>
    /// <param name="tile"> A tile within the multi-tile. </param>
    /// <param name="x"> The x coordinate of the specified tile. </param>
    /// <param name="y"> The y coordinate of the specified tile. </param>
    /// <param name="multiTileWidth">
    /// The width of the full multi-tile, in tiles.
    /// Assumes multi-tile sprite uses 16x16 for each tile frame, with 2 pixels worth of padding.
    /// </param>
    //TODO: Replace all references to GetCornerOfMultiTile and delete this method
    public static Point GetTopLeftOfMultiTile(Tile tile, int x, int y, int multiTileWidth) => GetCornerOfMultiTile(tile, x, y, CornerType.TopLeft);

    public static Point GetCornerOfMultiTile(Tile tile, int x, int y, CornerType corner) {
        TileObjectData data = TileObjectData.GetTileData(tile);
        Point topLeft = new(x - tile.TileFrameX % data.CoordinateFullWidth / 18, y - tile.TileFrameY % data.CoordinateFullHeight / 18);

        return corner switch {
            CornerType.TopLeft => topLeft,
            CornerType.TopRight => topLeft + new Point(data.Width - 1, 0),
            CornerType.BottomLeft => topLeft + new Point(0, data.Height - 1),
            CornerType.BottomRight => topLeft + new Point(data.Width - 1, data.Height - 1),
            _ => topLeft
        };
    }

    /// <summary>
    /// Method that starts at a specified initial tile position, and moves down until the passed in conditional is
    /// satisfied. Returns the first point that meets the conditions, or if one isn't found (or if the maximum drop is
    /// reached), returns null.
    /// </summary>
    /// <param name="condition"> The condition function that will determine if a tile position is valid. </param>
    /// <param name="initialPoint"> The initial tile point to start searching from. </param>
    /// <param name="maximumDrop">
    /// The maximum distance from the initial Y point. Exceeding will forcefully trigger
    /// failure.
    /// </param>
    /// <returns></returns>
    public static Point? DropUntilCondition(Func<Point, bool> condition, Point initialPoint, int maximumDrop) {
        Point point = new(initialPoint.X, initialPoint.Y);

        for (int i = 0; i <= maximumDrop; i++) {
            if (condition(point)) {
                return point;
            }

            point.Y++;
        }

        return null;
    }
}