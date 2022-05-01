using Microsoft.Xna.Framework;
using Terraria;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.VanillaOverrides.WorldGen.GenShapes {
    /// <summary>
    /// Simple GenShape that is basically a wrapper for a vanilla Utils method for simplification purposes.
    /// </summary>
    public class StraightLine : GenShape {
        private readonly float _width;
        private readonly Vector2 _endOffset;

        public StraightLine(float width, Vector2 endOffset) {
            _width = width * 16f;
            _endOffset = endOffset * 16f;
        }

        public override bool Perform(Point origin, GenAction action) {
            Vector2 start = new Vector2(origin.X << 4, origin.Y << 4);

            return Utils.PlotTileLine(start, start + _endOffset, _width, (x, y) => UnitApply(action, origin, x, y) || !_quitOnFail);
        }
    }
}