using Terraria.DataStructures;

namespace LivingWorldMod.Custom.Classes {
    /// <summary>
    /// Small class that holds data on a given door position, and what
    /// other door that its linked to.
    /// </summary>
    public sealed class PyramidDoorData {
        /// <summary>
        /// The position of THIS door.
        /// </summary>
        public Point16 doorPos;

        /// <summary>
        /// The data of the other door THIS door is linked to.
        /// </summary>
        public PyramidDoorData linkedDoor;
    }
}