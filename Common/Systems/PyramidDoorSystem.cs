using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// ModSystem that acts as the heart of the pyramid door opening shenanigans.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class PyramidDoorSystem : ModSystem {
        /// <summary>
        /// The "Phase" of the opening animation the specified door is currently in.
        /// </summary>
        public int DoorAnimationPhase {
            get;
            private set;
        }

        /// <summary>
        /// The position in tiles of the door currently being opened to enter.
        /// </summary>
        public Point16 DoorBeingOpenedPosition {
            get;
            private set;
        } = Point16.NegativeOne;

        /// <summary>
        /// The timer for the door opening process.
        /// </summary>
        public int DoorAnimationTimer {
            get;
            private set;
        }


        public const int PlayerWalkingIntoDoorPhase = 6;

        public override void PostUpdateEverything() {
            if (DoorBeingOpenedPosition == Point16.NegativeOne) {
                return;
            }

            if (DoorAnimationPhase < PlayerWalkingIntoDoorPhase) {
                if (++DoorAnimationTimer >= 75) {
                    DoorAnimationPhase++;
                    DoorAnimationTimer = 0;
                }
            }
            else if (DoorAnimationPhase == PlayerWalkingIntoDoorPhase) {
                //Check out PyramidAnimationPlayer.cs in Common/Players for more on the drawing specifically.
                if (++DoorAnimationTimer >= 240) {
                    DoorBeingOpenedPosition = Point16.NegativeOne;
                    DoorAnimationPhase = 0;
                    DoorAnimationTimer = 0;
                }
            }
        }

        public void StartDoorOpen(Point16 doorPosition) {
            if (DoorBeingOpenedPosition != Point16.NegativeOne) {
                return;
            }

            DoorBeingOpenedPosition = doorPosition;
            DoorAnimationTimer = 0;
            DoorAnimationPhase = 1;
        }
    }
}