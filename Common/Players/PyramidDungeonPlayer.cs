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
        public List<PyramidRoomCurse> CurrentCurses => currentRoom?.roomCurses ?? new List<PyramidRoomCurse>();

        public override void PostUpdate() {
            if (!SubworldSystem.IsActive<PyramidSubworld>()) {
                currentRoom = null;
                return;
            }

            currentRoom = ModContent.GetInstance<PyramidSubworld>().Grid.GetPlayersCurrentRoom(Player);
        }

        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
            foreach (PyramidRoomCurse curse in CurrentCurses) {
                curse.PlayerHurt(Player, pvp, quiet, damage, hitDirection, crit, cooldownCounter);
            }
        }

        public override void PreUpdateBuffs() {
            foreach (PyramidRoomCurse curse in CurrentCurses) {
                curse.PlayerPreUpdateBuffs(Player);
            }
        }

        public override void PostUpdateEquips() {
            foreach (PyramidRoomCurse curse in CurrentCurses) {
                curse.PlayerUpdateEquips(Player);
            }
        }

        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            foreach (PyramidRoomCurse curse in CurrentCurses) {
                curse.PlayerModifyShoot(Player, item, ref position, ref velocity, ref type, ref damage, ref knockback);
            }
        }
    }
}