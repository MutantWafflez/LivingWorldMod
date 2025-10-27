
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Globals.BaseTypes.Walls;

/// <summary>
///     Base class for all LWM Walls that has some functionality for the streamlining of the wall
///     creation process.
/// </summary>
public abstract class BaseWall : ModWall {
    /// <summary>
    ///     What Color is used to display this Wall on the Map.
    ///     Return null if you wish for this wall to not be displayed
    ///     on the map at all. Returns null by default.
    /// </summary>
    /// <remarks>
    ///     Make sure to call base.SetStaticDefaults() if you override SetStaticDefaults, so the
    ///     automatic map handling is run.
    /// </remarks>
    public virtual Color? WallColorOnMap => null;

    public override string Texture => GetType()
            .Namespace?
            .Replace($"{nameof(LivingWorldMod)}.Content.", LWM.SpritePath)
            .Replace('.', '/')
        + $"/{Name}";

    public override void SetStaticDefaults() {
        LWMUtils.TryAddMapEntry(this, Type, WallColorOnMap);
    }
}