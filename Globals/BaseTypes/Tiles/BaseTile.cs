using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Globals.BaseTypes.Tiles;

/// <summary>
///     Simple class that is the base class for all tiles in this mod. Has a couple pieces of functionality
///     to make the tile creation process easier.
/// </summary>
public abstract class BaseTile : ModTile {
    /// <summary>
    ///     What Color is used to display this tile on the Map.
    ///     Return null if you wish for this tile to not be displayed
    ///     on the map at all. Returns null by default.
    /// </summary>
    public virtual Color? TileColorOnMap => null;

    public override string Texture => GetType()
            .Namespace?
            .Replace($"{nameof(LivingWorldMod)}.Content.", LWM.SpritePath)
            .Replace('.', '/')
        + $"/{Name}";

    public override void PostSetDefaults() {
        LWMUtils.TryAddMapEntry(this, Type, TileColorOnMap);
    }
}