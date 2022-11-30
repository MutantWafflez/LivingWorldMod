using Microsoft.Xna.Framework;
using Terraria;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.VanillaOverrides.WorldGen.GenShapes {
    /// <summary>
    /// Simple GenShape that is basically a wrapper for a vanilla Utils method for simplification purposes.
    /// </summary>
    public class StraightLine : GenShape {
        private readonly float _width;
        private readonly Point _endPosition;

        /// <param name="width"> The width in TILES you want the line to be. </param>
        /// <param name="endPosition">
        /// The position in TILES of where you want the line to navigate towards, starting from the
        /// start position.
        /// </param>
        public StraightLine(float width, Point endPosition) {
            _width = width * 16f;
            _endPosition = endPosition;
        }

        public override bool Perform(Point origin, GenAction action) {
            Vector2 start = new(origin.X << 4, origin.Y << 4);

            return Utils.PlotTileLine(start, _endPosition.ToWorldCoordinates(0f, 0f), _width, (x, y) => UnitApply(action, origin, x, y) || !_quitOnFail);
        }
    }
}