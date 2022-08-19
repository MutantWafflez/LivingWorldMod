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
        /// <summary>
        /// A dynamically modified list that contains all of this player's currently equipped accessories from
        /// this mod.
        /// </summary>
        public List<AccessoryItem> equippedModAccessories = new List<AccessoryItem>();

        /// <summary>
        /// A list reset every frame that holds data for active miscellaneous effects not explicitly handled
        /// by the hooks in this class.
        /// </summary>
        public List<string> activeMiscEffects = new List<string>();

        public override void ResetEffects() {
            foreach (AccessoryItem accessory in equippedModAccessories) {
                accessory.ResetAccessoryEffects(Player);
            }
            equippedModAccessories = new List<AccessoryItem>();
            activeMiscEffects = new List<string>();
        }

        public override void UpdateDead() {
            equippedModAccessories = new List<AccessoryItem>();
            activeMiscEffects = new List<string>();
        }

        public override void PostUpdateEquips() {
            equippedModAccessories = equippedModAccessories.OrderBy(item => item.EffectPriority).ToList();
        }


        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            bool returnValue = base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);

            foreach (AccessoryItem accessory in equippedModAccessories) {
                if (accessory.PrePlayerKill(Player, damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource) || !returnValue) {
                    continue;
                }

                returnValue = false;
                foreach (AccessoryItem accessoryTwo in equippedModAccessories) {
                    accessoryTwo.PlayerDeathNegated(Player);
                }
            }

            return returnValue;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
            foreach (AccessoryItem accessory in equippedModAccessories) {
                accessory.PlayerKill(Player, damage, hitDirection, pvp, damageSource);
            }
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
            bool returnValue = base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);

            foreach (AccessoryItem accessory in equippedModAccessories) {
                if (accessory.PrePlayerHurt(Player, pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter) || !returnValue) {
                    continue;
                }

                returnValue = false;
            }

            return returnValue;
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
            foreach (AccessoryItem accessory in equippedModAccessories) {
                accessory.PlayerHurt(Player, pvp, quiet, damage, hitDirection, crit, cooldownCounter);
            }
        }

        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
            foreach (AccessoryItem accessory in equippedModAccessories) {
                accessory.PostPlayerHurt(Player, pvp, quiet, damage, hitDirection, crit, cooldownCounter);
            }
        }
    }
}