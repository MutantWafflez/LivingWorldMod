using Terraria.WorldBuilding;

namespace LivingWorldMod.Custom.Classes.WorldGen.GenConditions {

    /// <summary>
    /// Simple GenCondition that checks whether or not the specified tile is air.
    /// </summary>
    public class IsAir : GenCondition {

        protected override bool CheckValidity(int x, int y) {
            return !_tiles[x, y].IsActive;
        }
    }
}