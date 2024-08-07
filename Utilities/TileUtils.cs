﻿using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Drawing;
using Terraria.ObjectData;

namespace LivingWorldMod.Utilities;

/// <summary>
///     Utilities class that deals with the Tile class and tiles in general.
/// </summary>
public static partial class LWMUtils {
    public enum CornerType : byte {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    ///     Allows for calling of the private method "AddSpecialPoint(int, int, int)" in <see cref="TileDrawing" />.
    /// </summary>
    public static readonly Delegate AddSpecialPoint = AddSpecialPoint =
        typeof(TileDrawing).GetMethod("AddSpecialPoint", BindingFlags.Instance | BindingFlags.NonPublic)!.CreateDelegate<Action<int, int, int>>(Main.instance.TilesRenderer);

    /// <summary>
    ///     Checks and returns whether or not the given tile type at the given position can merge
    ///     with the other tile at the position offset by the given offset. For example, passing the
    ///     dirt tile type and its position and an offset of 0, 1 would check if dirt can merge with
    ///     the tile right below it.
    /// </summary>
    /// <param name="tileType"> The tile type at the given position. </param>
    /// <param name="tilePosition"> The position of the tile in question. </param>
    /// <param name="otherTileOffset">
    ///     The offset of the other tile in regards to the initial tile's position.
    /// </param>
    /// <returns> </returns>
    public static bool CanMergeWithTile(int tileType, Point tilePosition, Point otherTileOffset) {
        Tile otherTile = Framing.GetTileSafely(tilePosition + otherTileOffset);

        if (otherTile is not { HasTile: true }) {
            return false;
        }

        return otherTile.TileType == tileType
            || Main.tileMerge[tileType][otherTile.TileType]
            || (otherTile.TileType == TileID.Dirt && (Main.tileMergeDirt[tileType] || TileID.Sets.ForcedDirtMerging[tileType]))
            || (TileID.Sets.MergesWithClouds[tileType] && TileID.Sets.Clouds[otherTile.TileType])
            || (TileID.Sets.OreMergesWithMud[tileType] && TileID.Sets.Mud[otherTile.TileType]);
    }

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
    ///     Method that starts at a specified initial tile position, and moves down until the passed in conditional is
    ///     satisfied. Returns the first point that meets the conditions, or if one isn't found (or if the maximum drop is
    ///     reached), returns null.
    /// </summary>
    /// <param name="condition"> The condition function that will determine if a tile position is valid. </param>
    /// <param name="initialPoint"> The initial tile point to start searching from. </param>
    /// <param name="maximumDrop">
    ///     The maximum distance from the initial Y point. Exceeding will forcefully trigger
    ///     failure.
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

    /// <summary>
    ///     Runs the given function for each tile inside the provided rectangle. If the function returns true on
    ///     a given index, then this method terminates.
    /// </summary>
    public static void DoInRectangle(Rectangle rectangle, Func<Point, bool> function) {
        for (int i = rectangle.X; i < rectangle.X + rectangle.Width; i++) {
            for (int j = rectangle.Y; j < rectangle.Y + rectangle.Height; j++) {
                if (function(new Point(i, j))) {
                    return;
                }
            }
        }
    }
}