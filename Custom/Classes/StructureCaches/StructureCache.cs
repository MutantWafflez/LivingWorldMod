namespace LivingWorldMod.Custom.Classes.StructureCaches {

    /// <summary>
    /// Okay, full send, I know this is probably a really disgusting way to go about this but I am
    /// really bad at World Gen so this is the best I got with what I know what to do. So, here you
    /// go. Anyways, as for what this actually does, there should be a child class for each
    /// structure that overrides each of the array properties for all the data required in the structure.
    /// </summary>
    public abstract class StructureCache {

        public abstract int[,] TileTypes {
            get;
        }

        public abstract bool[,] AreTilesHalfBlocks {
            get;
        }

        public abstract int[,] TileFrameNumbers {
            get;
        }

        public abstract int[,] TileFrameXs {
            get;
        }

        public abstract int[,] TileFrameYs {
            get;
        }

        public abstract int[,] TileSlopeTypes {
            get;
        }

        public abstract int[,] TileColors {
            get;
        }

        public abstract bool[,] AreTilesActuated {
            get;
        }

        public abstract bool[,] HaveActuators {
            get;
        }

        public abstract bool[,] HaveRedWires {
            get;
        }

        public abstract bool[,] HaveBlueWires {
            get;
        }

        public abstract bool[,] HaveGreenWires {
            get;
        }

        public abstract bool[,] HaveYellowWires {
            get;
        }

        public abstract int[,] LiquidTypes {
            get;
        }

        public abstract int[,] LiquidAmounts {
            get;
        }

        public abstract int[,] WallTypes {
            get;
        }

        public abstract int[,] WallColors {
            get;
        }

        public abstract int[,] WallFrameNumbers {
            get;
        }

        public abstract int[,] WallFrameXs {
            get;
        }

        public abstract int[,] WallFrameYs {
            get;
        }
    }
}