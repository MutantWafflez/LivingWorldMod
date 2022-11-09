using System.Collections.Generic;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {
    /// <summary>
    /// ModPlayer that handles player related things with the Revamped Pyramid dungeon.
    /// </summary>
    public class PyramidDungeonPlayer : ModPlayer {
        /// <summary>
        /// The pyramid room this player is currently in.
        /// </summary>
        public PyramidRoom currentRoom;

        /// <summary>
        /// Reference to this player's current list of curses in accordance to
        /// the room they are in.
        /// </summary>
        public List<PyramidRoomCurseType> CurrentCurses => currentRoom?.roomCurses ?? new List<PyramidRoomCurseType>();

        public override void PostUpdate() {
            if (!SubworldSystem.IsActive<PyramidSubworld>()) {
                currentRoom = null;
                return;
            }

            currentRoom = ModContent.GetInstance<PyramidSubworld>().Grid.GetPlayersCurrentRoom(Player);
        }
    }
}