using LivingWorldMod.Content.Tiles.Interactables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Cutscenes {
    /// <summary>
    /// Cutscene that plays on the doors within the actual pyramid itself for navigation between rooms.
    /// </summary>
    public class InnerPyramidDoorCutscene : EnterPyramidCutscene {
        /// <summary>
        /// The position this player will end up when this cutscene finishes.
        /// </summary>
        public Vector2 EndTeleportPosition {
            get;
        }

        public override PyramidDoorTile DoorModTile => ModContent.GetInstance<InnerPyramidDoorTile>();

        /// <summary>
        /// How many ticks it takes for the door to visually update from phase to phase.
        /// </summary>
        public override int DoorOpeningRate => 20;

        /// <summary>
        /// How many ticks it takes before the player begins visually entering the door after the door
        /// has completely opened.
        /// </summary>
        public override int WaitTimeBeforeWalkIn => 20;

        /// <summary>
        /// How long it takes for the player to fully "walk into" the open door.
        /// </summary>
        public override int PlayerWalkingTime => 80;

        public InnerPyramidDoorCutscene(Point16 doorPos, Vector2 teleportPos) : base(doorPos) {
            EndTeleportPosition = teleportPos;
        }

        public override void OnFinish(Player player) {
            if (Main.netMode == NetmodeID.Server) {
                return;
            }
            DoorModTile.playerInCutscene = null;
            if (player.whoAmI != Main.myPlayer) {
                return;
            }

            player.Teleport(EndTeleportPosition, -1);
            NetMessage.SendData(MessageID.Teleport, number: 0, number2: player.whoAmI, number3: EndTeleportPosition.X, number4: EndTeleportPosition.Y, number5: -1);
        }
    }
}