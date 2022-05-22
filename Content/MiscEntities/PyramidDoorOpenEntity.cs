using Terraria.DataStructures;

namespace LivingWorldMod.Content.MiscEntities {
    /// <summary>
    /// Psuedo-Entity that assists with the opening process/animation of the Pyramid Door Tile.
    /// Purely exists in order to facilitate only client-side animation playing; Tile Entities
    /// in multiplayer environments only update on the server-side, so we have to make this workaround.
    /// </summary>
    public sealed class PyramidDoorOpenEntity {
        /// <summary>
        /// Whether or not the opening process for this specific entity has finished.
        /// </summary>
        public bool isFinished;

        private Point16 _position;
        private int _activationVFXStage;
        private int _activationVFXTimer;

        public PyramidDoorOpenEntity(Point16 position) {
            _position = position;
        }

        public void Update() {
            if (isFinished) {
                return;
            }
        }
    }
}