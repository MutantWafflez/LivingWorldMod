using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// ModSystem that acts as the heart of the pyramid door opening shenanigans.
    /// </summary>
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

        /// <summary>
        /// The timer for the door opening process.
        /// </summary>
        public int DoorOpeningTimer {
            get;
            private set;
        }

        public override void PostUpdateEverything() {
            if (DoorBeingOpened == Point16.NegativeOne) {
                return;
            }

            if (DoorOpeningPhase < 5) {
                if (++DoorOpeningTimer >= 75) {
                    DoorOpeningPhase++;
                    DoorOpeningTimer = 0;
                }
            }
            else if (DoorOpeningPhase == 5) {
                //Check out PyramidAnimationPlayer.cs in Common/Players for more on the drawing specifically.
                if (++DoorOpeningTimer >= 240) {
                    DoorBeingOpened = Point16.NegativeOne;
                    DoorOpeningPhase = 0;
                    DoorOpeningTimer = 0;
                }
            }
        }

        public void StartDoorOpen(Point16 doorPosition) {
            if (DoorBeingOpened != Point16.NegativeOne) {
                return;
            }

            DoorBeingOpened = doorPosition;
            DoorOpeningTimer = 0;
            DoorOpeningPhase = 0;
        }
    }
}