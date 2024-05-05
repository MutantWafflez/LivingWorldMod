using LivingWorldMod.Globals.Patches;
using Terraria.ObjectData;

namespace LivingWorldMod.Globals.Sets;

/// <summary>
/// Class that acts as a psuedo-extension of vanilla's <seealso cref="TileID.Sets"/>,
/// where-in the arrays in this class function for purely this mod's purposes.
/// </summary>
public static class TileSets {
    /// <summary>
    /// Whether this tile needs "advanced" wind sway, i.e needs to be handled in the
    /// <see cref="TileDrawingPatches.WindDrawingEdit"/> method to function properly.
    /// Basically, this is the more advanced version of the <see cref="TileID.Sets.SwaysInWindBasic"/> set.
    /// </summary>
    /// <remarks>
    /// Needs an associated <see cref="TileObjectData"/> instance to function at all, as the proper values are set using the
    /// width and height properties. Furthermore, a special point must be added during the PreDraw step. See
    /// <see cref="GravityTapestryTile"/> for an example.
    /// </remarks>
    public static bool[] NeedsAdvancedWindSway = Factory.CreateBoolSet(false);

    public static SetFactory Factory => TileID.Sets.Factory;
}