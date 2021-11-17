using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.VanillaOverrides.WorldGen.GenConditions {
    /// <summary>
    /// Simple GenCondition that checks whether or not the specified tile is air.
    /// </summary>
    public class IsAir : GenCondition {
        protected override bool CheckValidity(int x, int y) => !_tiles[x, y].IsActive;
    }
}