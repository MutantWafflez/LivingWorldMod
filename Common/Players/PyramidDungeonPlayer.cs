using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Subworlds.Pyramid;
using LivingWorldMod.Core.PacketHandlers;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
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

        public const int MaxPlaguesCurseTimer = 60 * 20;

        /// <summary>
        /// Cached list of all debuffs.
        /// </summary>
        private static List<int> _allDebuffs;

        /// <summary>
        /// Reference to this player's current list of curses in accordance to
        /// the room they are in.
        /// </summary>
        public List<PyramidRoomCurseType> CurrentCurses => currentRoom?.ActiveCurses ?? new List<PyramidRoomCurseType>();

        /// <summary>
        /// The pyramid room this player is currently in.
        /// </summary>
        public PyramidRoom currentRoom;

        /// <summary>
        /// List of all currently pending heals due to the Curse of Delay.
        /// </summary>
        public List<HealInstance> healDelays = new();

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
                    case PyramidRoomCurseType.Nyctophobia:
                        if (Main.myPlayer == Player.whoAmI) {
                            DontStarveDarknessDamageDealer.Update(Player);
                        }
                        break;
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

        public override void PostUpdateEquips() {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Impact:
                        Player.noKnockback = false;
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
                    case PyramidRoomCurseType.Proximity:
                        for (int i = 0; i < Main.maxNPCs; i++) {
                            NPC npc = Main.npc[i];
                            if (!npc.active || npc.friendly || Player.Center.Distance(npc.Center) >= 16f * 15f) {
                                continue;
                            }

                            Player.maxRunSpeed = 3f;
                            Player.accRunSpeed = 3f;
                            Player.runAcceleration = 0.05f;
                            break;
                        }
                        break;
                }
            }
        }

        public override void PostUpdate() {
            if (!SubworldSystem.IsActive<PyramidSubworld>()) {
                currentRoom = null;
                return;
            }

            currentRoom = ModContent.GetInstance<PyramidSubworld>().grid.GetEntityCurrentRoom(Player);
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

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Chaos:
                        if (!quiet) {
                            damage = (int)(damage * Main.rand.NextFloat(0.25f, 2.5f));
                        }
                        break;
                }
            }

            return true;
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
                    case PyramidRoomCurseType.Impact:
                        if (!Player.noKnockback && hitDirection != 0 && (!Player.mount.Active || !Player.mount.Cart)) {
                            Player.velocity *= 2f;
                        }
                        break;
                    case PyramidRoomCurseType.Thievery:
                        if (Main.myPlayer == Player.whoAmI) {
                            ref Item[] inventory = ref Player.inventory;
                            bool coinsWereLost = false;

                            for (int i = 0; i < 59; i++) {
                                if (!inventory[i].IsACoin) {
                                    continue;
                                }
                                coinsWereLost = true;

                                inventory[i].stack -= (int)(inventory[i].stack * 0.25f);

                                if (i == 58) {
                                    Main.mouseItem = inventory[i].Clone();
                                }
                            }

                            if (coinsWereLost) {
                                SoundEngine.PlaySound(SoundID.Coins, Player.Center);
                            }
                        }
                        break;
                    case PyramidRoomCurseType.Disarmament:
                        if (Main.netMode == NetmodeID.Server) {
                            break;
                        }
                        AccessoryPlayer accessoryPlayer = Player.GetModPlayer<AccessoryPlayer>();

                        List<int> accessorySlots = new() { 3, 4, 5, 6, 7 };
                        if (Main.masterMode || Player.extraAccessory && Main.expertMode) {
                            accessorySlots.Add(8);
                        }
                        if (Main.masterMode && Player.extraAccessory) {
                            accessorySlots.Add(9);
                        }

                        accessorySlots.RemoveAll(slot => accessoryPlayer.disabledAccessorySlots.Any(instance => instance.typeOrSlot == slot));
                        if (!accessorySlots.Any()) {
                            break;
                        }

                        int selectedSlot = Main.rand.Next(accessorySlots);
                        AccessoryPlayer.DisabledAccessoryInstance instance = new(30 * 60, false, selectedSlot);
                        accessoryPlayer.disabledAccessorySlots.Add(instance);

                        if (Main.netMode == NetmodeID.MultiplayerClient) {
                            ModPacket packet = ModContent.GetInstance<AccessoryPacketHandler>().GetPacket(AccessoryPacketHandler.AddDisabledAccessorySlot);
                            AccessoryPacketHandler.WriteDisabledAccessoryInstance(instance, packet);

                            packet.Send();
                        }
                        break;
                    case PyramidRoomCurseType.Recursion:
                        if (Main.netMode != NetmodeID.MultiplayerClient && currentRoom.ActiveCurses.Count < 5) {
                            currentRoom.AddRandomCurse();
                        }
                        break;
                }
            }
        }
    }
}