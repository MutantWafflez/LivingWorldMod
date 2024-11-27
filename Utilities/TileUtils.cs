using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Drawing;
using Terraria.Localization;
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

    /// <summary>
    ///     A more "auto-magic" version of <see cref="AddMapEntry" />, where assumptions are automatically made about what kind of map entry is intended to be added, and where.
    ///     <para></para>
    ///     If the <see cref="instance" /> parameter is a <see cref="ModTile" /> instance, it will be assumed that <see cref="MapLoader.tileEntries" /> is going to be added to. The same applies for
    ///     <see cref="ModWall" /> and <see cref="MapLoader.wallEntries" />. This method will fail and do nothing if the <see cref="instance" /> parameter is neither tile nor wall.
    ///     <para></para>
    ///     The "hover text" functionality on the map will only be added if an existing localization key exists, following the format that <see cref="ILocalizedModTypeExtensions.GetLocalizationKey" /> uses,
    ///     with the suffix of "MapEntry". For example: Mods.LivingWorldMod.Tiles.ExampleTile.MapEntry. If the given key does not exist, no hover text will be added.
    ///     <para></para>
    ///     If the provided <see cref="color" /> parameter is null, this function will fail.
    /// </summary>
    /// <returns>Whether a map entry was successfully added.</returns>
    public static bool TryAddMapEntry(ILocalizedModType instance, ushort type, Color? color) {
        if (MapLoader.initialized || color is not { } mapColor) {
            return false;
        }

        IDictionary<ushort, IList<MapEntry>> entryDict;
        switch (instance) {
            case ModTile:
                entryDict = MapLoader.tileEntries;
                break;
            case ModWall:
                entryDict = MapLoader.wallEntries;
                break;
            default:
                return false;
        }

        string mapEntryKey = instance.GetLocalizationKey("MapEntry");
        //AKA check if the localization for this tile exists, and only add it if it does
        //Translations will return the key if you try to get the translation value for a translation that doesn't exist.
        AddMapEntry(entryDict, type,  mapColor, mapEntryKey == Language.GetTextValue(mapEntryKey) ? null : Language.GetText(mapEntryKey));
        return true;
    }

    /// <summary>
    ///     Method that allow for adding a map entry to a dictionary of <see cref="MapEntry" /> lists. Applicable for <see cref="MapLoader.tileEntries" /> and <see cref="MapLoader.wallEntries" /> for
    ///     adding map entries for tiles and walls, respectively.
    /// </summary>
    /// <remarks>
    ///     I created this because <see cref="ModTile.AddMapEntry" /> and <see cref="ModWall.AddMapEntry" /> could easily be coalesced into the same method, where the passed in dictionary is the only thing
    ///     that changes between them. Also exists because of all the red tape in terms of access levels (everything is either internal or private).
    /// </remarks>
    public static void AddMapEntry(IDictionary<ushort, IList<MapEntry>> entryDict, ushort type, Color color, LocalizedText hoverText = null) {
        MapEntry mapEntry = new(color, hoverText);
        if (!entryDict.TryGetValue(type, out IList<MapEntry> list)) {
            list = new List<MapEntry>();
            entryDict[type] = list;
        }

        list.Add(mapEntry);
    }
}