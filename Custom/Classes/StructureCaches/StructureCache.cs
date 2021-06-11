namespace LivingWorldMod.Custom.Classes.StructureCaches {

    /// <summary>
    /// Okay, full send, I know this is probably a really disgusting way to go about this but I am
    /// really bad at World Gen so this is the best I got with what I know what to do. So, here you
    /// go. Anyways, as for what this actually does, create an instance of the class and use the
    /// proper generation method to generate all of the array data needed to generate the given structure.
    /// </summary>
    public sealed partial class StructureCache {
        public int[,] currentTileArray;

        public int[,] currentSlopeArray;

        public int[,] currentWallArray;

        public int[,] currentLiquidArray;

        public int[,] currentXFrameArray;

        public int[,] currentYFrameArray;
    }
}