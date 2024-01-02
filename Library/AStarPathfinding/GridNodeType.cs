namespace LivingWorldMod.Library.AStarPathfinding;

/// <summary>
/// Enum that describes the "type" of a node within the
/// pathfinding grid.
/// </summary>
public enum GridNodeType : byte {
    /// <summary>
    /// For tiles/spaces that have no collision in any circumstance what-so-ever.
    /// This can be air, an actuated tile, or a passable liquid (with no tile).
    /// Nodes of this type have an associated weight.
    /// </summary>
    NonSolid,

    /// <summary>
    /// For tiles/spaces that are completely solid, and cannot be moved through.
    /// This includes non-actuated solid tiles.
    /// </summary>
    Solid,

    /// <summary>
    /// For tiles/spaces that can't be navigated through, but aren't necessarily
    /// solid.
    /// </summary>
    Impassable,

    /// <summary>
    /// For tiles/spaces that have collision from the top only. This primarily includes platforms.
    /// Nodes of this type have an associated weight.
    /// </summary>
    SolidTop
}