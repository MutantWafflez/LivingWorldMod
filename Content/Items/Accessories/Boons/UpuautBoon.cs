using LivingWorldMod.Content.StatusEffects.Buffs.Boons;
using LivingWorldMod.Content.StatusEffects.Debuffs.Boons;
using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Accessories.Boons {
    /// <summary>
    /// Another "death" related Boon; this one temporarily negates death
    /// and lets the player go into the Blaze of Glory for a little while,
    /// and then die.
    /// </summary>
    public class UpuautBoon : BoonAccessoryItem {
        public override string Texture => "Terraria/Images/NPC_Head_37";

        /// <summary>
        /// Whether or not the player is currently in a Blaze of Glory.
        /// </summary>
        private bool _inBlazeOfGlory;

        public override void SetDefaults() {
            Item.DefaultToAccessory();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override void AccessoryUpdate(Player player, bool hideVisual) {
            if (!player.HasBuff<UpuautDeathBuff>() && _inBlazeOfGlory) {
                _inBlazeOfGlory = false;

                player.AttemptForceKill(PlayerDeathReason.ByCustomReason("Boon"));
            }
        }

        public override bool PrePlayerKill(Player player, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            if (player.HasBuff<AnubisBoonNegateDebuff>()) {
                return true;
            }

            player.AddBuff(ModContent.BuffType<UpuautDeathBuff>(), 60 * (IsPotent ? 10 : 5));
            _inBlazeOfGlory = true;

            return false;
        }

        public override bool PrePlayerHurt(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
            if (player.statLife <= player.statLifeMax2 * (IsPotent ? 0.4f : 0.3f)) {
                damage = (int)(damage * (IsPotent ? 1.45f : 1.35f));
            }

            return true;
        }
    }
}