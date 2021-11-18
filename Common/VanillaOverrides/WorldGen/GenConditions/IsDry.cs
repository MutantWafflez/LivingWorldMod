using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.VanillaOverrides.WorldGen.GenConditions {
    /// <summary>
    /// Simple GenCondition that checks whether or not the specified tile has no liquid of any type.
    /// </summary>
    public class IsDry : GenCondition {
        protected override bool CheckValidity(int x, int y) => _tiles[x, y].LiquidAmount == 0;
    }
}