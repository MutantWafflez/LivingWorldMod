using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Items.Accessories;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {
    /// <summary>
    /// ModPlayer that handles the accessories in this mod.
    /// </summary>
    public class AccessoryPlayer : ModPlayer {
        private static HashSet<int> allModAccessoryTypes;

        /// <summary>
        /// A list of actual AccessoryItem objects that are currently equipped.
        /// </summary>
        public List<AccessoryItem> ActiveAccessoryItems {
            get;
            private set;
        }

        /// <summary>
        /// Dictionary of item type to bool pairs that designate which
        /// effects are currently active. Exists to allow for one
        /// accessory to enabled two other effects, for example.
        /// </summary>
        public Dictionary<int, bool> activeAccessoryEffects;

        public override void Initialize() {
            activeAccessoryEffects = new Dictionary<int, bool>();
        }

        public override void SetStaticDefaults() {
            allModAccessoryTypes = ModContent.GetContent<AccessoryItem>().Select(item => item.Type).ToHashSet();
        }

        public override void Unload() {
            allModAccessoryTypes = null;
        }

        public override void ResetEffects() {
            ResetActiveAccessoryEffects();
        }

        public override void UpdateDead() {
            ResetActiveAccessoryEffects();
        }

        public override void PostUpdateEquips() {
            ActiveAccessoryItems = new List<AccessoryItem>();

            //Vanilla slot checks
            for (int accIndex = 3; accIndex < 10; accIndex++) {
                if (Player.IsAValidEquipmentSlotForIteration(accIndex) && Player.armor[accIndex].ModItem is AccessoryItem item) {
                    ActiveAccessoryItems.Add(item);
                }
            }

            //Modded slot checks
            foreach (ModAccessorySlot accSlot in ModContent.GetContent<ModAccessorySlot>()) {
                if (accSlot.IsEnabled() && accSlot.FunctionalItem.ModItem is AccessoryItem item) {
                    ActiveAccessoryItems.Add(item);
                }
            }

            //Checks for any stragglers in activeAccessoryEffects
            foreach (int accType in allModAccessoryTypes.Where(accType => activeAccessoryEffects[accType])) {
                ModItem modItem = ModContent.GetModItem(accType);
                if (modItem is AccessoryItem accItem && ActiveAccessoryItems.All(item => item.Type != accType)) {
                    ActiveAccessoryItems.Add(accItem);
                }
            }

            ActiveAccessoryItems = ActiveAccessoryItems.OrderByDescending(item => item.EffectPriority).ToList();
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            bool returnValue = base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);

            foreach (AccessoryItem accessory in ActiveAccessoryItems) {
                if (accessory.PrePlayerKill(Player, damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource) || !returnValue) {
                    continue;
                }

                returnValue = false;
                foreach (AccessoryItem accessoryTwo in ActiveAccessoryItems) {
                    accessoryTwo.PlayerDeathNegated(Player);
                }
            }

            return returnValue;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
            foreach (AccessoryItem accessory in ActiveAccessoryItems) {
                accessory.PlayerKill(Player, damage, hitDirection, pvp, damageSource);
            }
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
            bool returnValue = base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);

            foreach (AccessoryItem accessory in ActiveAccessoryItems) {
                if (accessory.PrePlayerHurt(Player, pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter) || !returnValue) {
                    continue;
                }

                returnValue = false;
            }

            return returnValue;
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
            foreach (AccessoryItem accessory in ActiveAccessoryItems) {
                accessory.PlayerHurt(Player, pvp, quiet, damage, hitDirection, crit, cooldownCounter);
            }
        }

        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
            foreach (AccessoryItem accessory in ActiveAccessoryItems) {
                accessory.PostPlayerHurt(Player, pvp, quiet, damage, hitDirection, crit, cooldownCounter);
            }
        }

        /// <summary>
        /// Resets all currently active accessory effects.
        /// </summary>
        private void ResetActiveAccessoryEffects() {
            foreach (int accessoryType in allModAccessoryTypes) {
                activeAccessoryEffects[accessoryType] = false;
            }
        }
    }
}