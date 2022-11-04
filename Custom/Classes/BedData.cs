using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ObjectData;

namespace LivingWorldMod.Custom.Classes {
    /// <summary>
    /// Small class that holds data on a given bed tile.
    /// </summary>
    public class BedData {

        /// <summary>
        /// The tile position of this bed.
        /// </summary>
        public readonly Point bedPosition;

        /// <summary>
        /// The direction of this bed. 1 is the head of the bed on the right,
        /// -1 is the head of the bed on left.
        /// </summary>
        public readonly int bedDirection;

        /// <summary>
        /// The tile style of this bed.
        /// </summary>
        public readonly int bedStyle;

        public BedData(Point bedPosition) {
            this.bedPosition = bedPosition;
            bedDirection = Main.tile[bedPosition].TileFrameX > 18 * 3 ? 1 : -1;
            bedStyle = TileObjectData.GetTileStyle(Main.tile[bedPosition]);
        }

    }
}
