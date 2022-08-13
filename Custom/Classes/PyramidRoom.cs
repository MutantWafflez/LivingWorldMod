using Microsoft.Xna.Framework;

namespace LivingWorldMod.Custom.Classes {
    /// <summary>
    /// Small class that exists for data storage on a given pyramid room.
    /// </summary>
    public sealed class PyramidRoom {
        /// <summary>
        /// The rectangle that denotes the actual tile position and width/length.
        /// </summary>
        public Rectangle region;

        /// <summary>
        /// The top left position's X value of this room in terms of the GRID, not tiles.
        /// </summary>
        public int gridTopLeftX;

        /// <summary>
        /// The top left position's Y value of this room in terms of the GRID, not tiles.
        /// </summary>
        public int gridTopLeftY;

        /// <summary>
        /// How wide this room is in terms of the GRID, not tiles.
        /// </summary>
        public int gridWidth;

        /// <summary>
        /// How tall this room is in terms of the GRID, not tiles.
        /// </summary>
        public int gridHeight;

        /// <summary>
        /// Whether or not this room has been searched in the process of creating the correct path for the dungeon.
        /// </summary>
        public bool pathSearched;

        public PyramidRoom(Rectangle region, int gridTopLeftX, int gridTopLeftY, int gridWidth, int gridHeight) {
            this.region = region;
            this.gridTopLeftX = gridTopLeftX;
            this.gridTopLeftY = gridTopLeftY;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
        }

        public override string ToString() => "{" + gridTopLeftX + ", " + gridTopLeftY + "} " + $"{gridWidth}x{gridHeight}";
    }
}