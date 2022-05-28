using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {
    [Autoload(Side = ModSide.Client)]
    public class PyramidDoorSystem : ModSystem {
        /// <summary>
        /// The "Phase" of the opening process the specified door is currently in.
        /// </summary>
        public int DoorOpeningPhase {
            get;
            private set;
        }

        /// <summary>
        /// The position in tiles of the door currently being opened to enter.
        /// </summary>
        public Point16 DoorBeingOpened {
            get;
            private set;
        } = Point16.NegativeOne;

        private int _doorOpeningTimer;

        public override void PostUpdateEverything() {
            if (DoorBeingOpened == Point16.NegativeOne) {
                return;
            }

            if (++_doorOpeningTimer >= 75) {
                DoorOpeningPhase++;
                _doorOpeningTimer = 0;
            }
            if (DoorOpeningPhase >= 6) {
                DoorBeingOpened = Point16.NegativeOne;
                DoorOpeningPhase = 0;
                _doorOpeningTimer = 0;
            }
        }

        public void StartDoorOpen(Point16 doorPosition) {
            if (DoorBeingOpened != Point16.NegativeOne) {
                return;
            }

            DoorBeingOpened = doorPosition;
            _doorOpeningTimer = 0;
            DoorOpeningPhase = 0;
        }
    }
}