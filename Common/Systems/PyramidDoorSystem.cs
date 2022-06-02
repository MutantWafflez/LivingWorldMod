using LivingWorldMod.Content.Subworlds;
using SubworldLibrary;
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

        /// <summary>
        /// The very last phase of the door animation before the door is completely open.
        /// </summary>
        public const int LastDoorAnimationPhase = 5;

        /// <summary>
        /// The phase that is simply a brief "pause" where the player stands still
        /// after the door is fully, visually open, before they actually enter the
        /// door.
        /// </summary>
        public const int PauseBeforePlayerWalkingPhase = 6;

        /// <summary>
        /// The phase of the door opening animation where the player is meant
        /// to visual "walk into" the open door.
        /// </summary>
        public const int PlayerWalkingIntoDoorPhase = 7;

        /// <summary>
        /// How many ticks it takes for the door to visually update from phase to phase.
        /// </summary>
        public const int DoorOpeningRate = 33;

        /// <summary>
        /// How many ticks it takes before the player begins visually entering the door after the door
        /// has completely opened.
        /// </summary>
        public const int WaitTimeBeforeWalkIn = 60;

        /// <summary>
        /// How long it takes for the player to fully "walk into" the open door.
        /// </summary>
        public const int PlayerWalkingTime = 240;

        public override void PostUpdateEverything() {
            if (DoorBeingOpenedPosition == Point16.NegativeOne) {
                return;
            }

            if (DoorAnimationPhase < PlayerWalkingIntoDoorPhase) {
                if (++DoorAnimationTimer >= (DoorAnimationPhase != PauseBeforePlayerWalkingPhase ? DoorOpeningRate : WaitTimeBeforeWalkIn)) {
                    DoorAnimationPhase++;
                    DoorAnimationTimer = 0;
                }
            }
            else if (DoorAnimationPhase == PlayerWalkingIntoDoorPhase) {
                //Check out PyramidAnimationPlayer.cs in Common/Players for more on the drawing specifically.
                if (++DoorAnimationTimer >= PlayerWalkingTime) {
                    DoorBeingOpenedPosition = Point16.NegativeOne;
                    DoorAnimationPhase = 0;
                    DoorAnimationTimer = 0;

                    SubworldSystem.Enter<PyramidDimension>();
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