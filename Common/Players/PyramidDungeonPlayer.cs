using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Subworlds.Pyramid;
using LivingWorldMod.Custom.Utilities;
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

        /// <summary>
        /// List of all currently pending heals due to the Curse of Delay.
        /// </summary>
        public List<HealInstance> healDelays = new List<HealInstance>();

        /// <summary>
        /// The countdown before The Plagues curse applies a new debuff.
        /// </summary>
        public int plaguesCurseTimer;

        /// <summary>
        /// The countdown before the Gravitational Instability curse swaps gravity for this player.
        /// </summary>
        public int gravitySwapTimer;

        /// <summary>
        /// The currently forced gravity direction.
        /// </summary>
        public float forcedGravityDirection = 1f;

        /// <summary>
        /// Reference to this player's current list of curses in accordance to
        /// the room they are in.
        /// </summary>
        public List<PyramidRoomCurseType> CurrentCurses => currentRoom?.roomCurses ?? new List<PyramidRoomCurseType>();

        /// <summary>
        /// Cached list of all debuffs.
        /// </summary>
        private static List<int> _allDebuffs;

        public const int MaxPlaguesCurseTimer = 60 * 20;

        public override void SetStaticDefaults() {
            _allDebuffs = new List<int>();

            for (int i = 0; i < Main.debuff.Length; i++) {
                _allDebuffs.AddConditionally(i, Main.debuff[i]);
            }
        }

        public override void ResetEffects() {
            if (SubworldSystem.IsActive<PyramidSubworld>()) {
                return;
            }

            plaguesCurseTimer = MaxPlaguesCurseTimer;
        }

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
                    case PyramidRoomCurseType.FrictionalIgnorance:
                        Player.slippy2 = true;
                        break;
                    case PyramidRoomCurseType.Viscosity:
                        Player.ignoreWater = false;
                        Player.accMerman = false;
                        Player.forceMerman = false;
                        Player.trident = false;
                        Player.canFloatInWater = false;
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
                    case PyramidRoomCurseType.ThePlagues:
                        if (Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer && --plaguesCurseTimer <= 0) {
                            plaguesCurseTimer = MaxPlaguesCurseTimer;

                            if (Player.statLife >= Player.statLifeMax2 * 0.33f) {
                                Player.AddBuff(Main.rand.Next(_allDebuffs), 60 * 10, false);
                            }
                        }
                        break;
                    case PyramidRoomCurseType.Obstruction:
                        Player.AddBuff(BuffID.Obstructed, 5);
                        break;
                    case PyramidRoomCurseType.Confusion:
                        Player.AddBuff(BuffID.Confused, 5);
                        break;
                }
            }
        }

        public override void PostUpdateMiscEffects() {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.ShatteredArmor:
                        Player.statDefense /= 2;
                        break;
                }
            }
        }

        public override void PostUpdateRunSpeeds() {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.GravitationalInstability:
                        Player.gravControl = false;
                        Player.gravControl2 = false;
                        break;
                }
            }
        }

        public override void PostUpdate() {
            if (!SubworldSystem.IsActive<PyramidSubworld>()) {
                currentRoom = null;
                return;
            }

            currentRoom = ModContent.GetInstance<PyramidSubworld>().Grid.GetEntityCurrentRoom(Player);
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

        public override float UseSpeedMultiplier(Item item) {
            float multiplier = 1f;

            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Lethargy:
                        multiplier -= 0.5f;
                        break;
                }
            }

            return multiplier;
        }

        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Ignition:
                        Player.AddBuff(BuffID.OnFire, 60 * 8);
                        break;
                    case PyramidRoomCurseType.Poison:
                        Player.AddBuff(BuffID.Poisoned, 60 * 8);
                        break;
                }
            }
        }
    }
}