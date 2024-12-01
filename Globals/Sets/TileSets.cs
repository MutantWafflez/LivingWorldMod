namespace LivingWorldMod.Globals.Sets;

/// <summary>
///     Class that acts as a psuedo-extension of vanilla's <seealso cref="TileID.Sets" />,
///     where-in the arrays in this class function for purely this mod's purposes.
/// </summary>
public static class TileSets {
    public static SetFactory Factory => TileID.Sets.Factory;
}