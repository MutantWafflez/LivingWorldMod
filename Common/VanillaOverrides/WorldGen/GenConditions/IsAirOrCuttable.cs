using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.VanillaOverrides.WorldGen.GenConditions {
    /// <summary>
    /// Simple GenCondition that checks whether or not the specified tile is air or a "tile" that can be cut, such as grass.
    /// </summary>
    public class IsAirOrCuttable : GenCondition {
        protected override bool CheckValidity(int x, int y) {
            Tile tile = _tiles[x, y];

            return !tile.IsActive
                   || Main.tileCut[tile.type]
                   || TileID.Sets.BreakableWhenPlacing[tile.type]
                   || TileID.Sets.IsVine[tile.type];
        }
    }
}