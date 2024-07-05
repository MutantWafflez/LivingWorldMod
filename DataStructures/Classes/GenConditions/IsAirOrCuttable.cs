using Terraria.WorldBuilding;

namespace LivingWorldMod.DataStructures.Classes.GenConditions;

/// <summary>
///     Simple GenCondition that checks whether or not the specified tile is air or a "tile" that can be cut, such as grass.
/// </summary>
public class IsAirOrCuttable : GenCondition {
    protected override bool CheckValidity(int x, int y) {
        Tile tile = _tiles[x, y];

        return !tile.HasTile
            || Main.tileCut[tile.TileType]
            || TileID.Sets.BreakableWhenPlacing[tile.TileType]
            || TileID.Sets.IsVine[tile.TileType];
    }
}