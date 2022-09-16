using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Custom.Classes {
    /// <summary>
    /// Small class that exists for data storage on a given pyramid room.
    /// </summary>
    public sealed class PyramidRoom {
        /// <summary>
        /// Small nested class that holds data on a given door position, and what
        /// other door that its linked to.
        /// </summary>
        public sealed class DoorData {
            /// <summary>
            /// The position of THIS door.
            /// </summary>
            public Point16 doorPos;

            /// <summary>
            /// The data of the other door THIS door is linked to.
            /// </summary>
            public DoorData linkedDoor;
        }

        /// <summary>
        /// The rectangle that denotes the actual tile position and width/length.
        /// </summary>
        public Rectangle region;

        /// <summary>
        /// Position of the top door which will lead to the room above this one.
        /// If this value is null, it means there is no top door for this room.
        /// </summary>
        public DoorData topDoor;

        /// <summary>
        /// Position of the right door which will lead to the room right of this one.
        /// If this value is null, it means there is no right door for this room.
        /// </summary>
        public DoorData rightDoor;

        /// <summary>
        /// Position of the left door which will lead to the room left os this one.
        /// If this value is null, it means there is no left door for this room.
        /// </summary>
        public DoorData leftDoor;

        /// <summary>
        /// Position of the down door which will lead to the room below this one.
        /// If this value is null, it means there is no down door for this room.
        /// </summary>
        public DoorData downDoor;

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

        /// <summary>
        /// Whether or not this room has been generated into the world.
        /// </summary>
        public bool worldGenned;

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