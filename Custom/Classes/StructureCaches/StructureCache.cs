namespace LivingWorldMod.Custom.Classes.StructureCaches {

    /// <summary>
    /// Okay, full send, I know this is probably a really disgusting way to go about this but I am
    /// really bad at World Gen so this is the best I got with what I know what to do. So, here you
    /// go. Anyways, as for what this actually does, there should be a child class for each
    /// structure that overrides each of the array properties for all the data required in the structure.
    /// </summary>
    public abstract class StructureCache {

        public abstract int[,] TileTypeArray {
            get;
        }

        public abstract int[,] SlopeTypeArray {
            get;
        }

        public abstract int[,] WallTypeArray {
            get;
        }

        public abstract int[,] LiquidAmountArray {
            get;
        }

        public abstract int[,] TileXFrameArray {
            get;
        }

        public abstract int[,] TileYFrameArray {
            get;
        }
    }
}