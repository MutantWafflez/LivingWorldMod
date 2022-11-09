using System.Collections.Generic;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {
    /// <summary>
    /// ModPlayer that handles player related things with the Revamped Pyramid dungeon.
    /// </summary>
    public class PyramidDungeonPlayer : ModPlayer {
        public class HealInstance {
            public int healTimer;
            public int healAmount;

            public HealInstance(int healTimer, int healAmount) {
                this.healTimer = healTimer;
                this.healAmount = healAmount;
            }
        }

        /// <summary>
        /// The pyramid room this player is currently in.
        /// </summary>
        public PyramidRoom currentRoom;

        public List<HealInstance> healDelays = new List<HealInstance>();

        /// <summary>
        /// Reference to this player's current list of curses in accordance to
        /// the room they are in.
        /// </summary>
        public List<PyramidRoomCurseType> CurrentCurses => currentRoom?.roomCurses ?? new List<PyramidRoomCurseType>();

        public override void UpdateDead() {
            healDelays.Clear();
        }

        public override void PreUpdate() {
            for (int i = 0; i < healDelays.Count; i++) {
                HealInstance instance = healDelays[i];

                if (--instance.healTimer > 0) {
                    continue;
                }
                Player.Heal(instance.healAmount);
                healDelays.Remove(instance);
                i--;
            }

            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Hemophilia:
                        Player.potionDelay = 2;
                        break;
                    case PyramidRoomCurseType.Grounding:
                        if (Player.gravDir > 0f ? Player.velocity.Y > 0f : Player.velocity.Y < 0f) {
                            Player.gravity *= 3;
                            Player.maxFallSpeed *= 2;
                        }
                        break;
                }
            }
        }

        public override void PreUpdateBuffs() {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Obstruction:
                        Player.AddBuff(BuffID.Obstructed, 5);
                        break;
                    case PyramidRoomCurseType.Confusion:
                        Player.AddBuff(BuffID.Confused, 5);
                        break;
                }
            }
        }

        public override void PostUpdate() {
            if (!SubworldSystem.IsActive<PyramidSubworld>()) {
                currentRoom = null;
                return;
            }

            currentRoom = ModContent.GetInstance<PyramidSubworld>().Grid.GetPlayersCurrentRoom(Player);
        }

        public override void UpdateBadLifeRegen() {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Hemophilia:
                        Player.lifeRegen = 0;
                        Player.lifeRegenTime = 0;
                        break;
                }
            }
        }

        public override void GetHealLife(Item item, bool quickHeal, ref int healValue) {
            if (quickHeal) {
                return;
            }

            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Hemophilia:
                        healValue = 0;
                        break;
                    case PyramidRoomCurseType.Delay:
                        healDelays.Add(new HealInstance(120, healValue));
                        healValue = 0;
                        break;
                }
            }
        }

        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Misfire:
                        if (Main.rand.NextBool(4)) {
                            velocity = velocity.RotateRandom(MathHelper.Pi / 6f);
                        }
                        break;
                }
            }
        }
    }
}