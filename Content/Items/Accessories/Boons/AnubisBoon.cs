using LivingWorldMod.Content.StatusEffects.Debuffs.Boons;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Accessories.Boons {
    /// <summary>
    /// Boon of Anubis, the one that does the death stuff.
    /// </summary>
    public class AnubisBoon : BoonAccessoryItem {
        //TODO: Get proper sprites for Boons!
        public override string Texture => "Terraria/Images/NPC_Head_38";

        public override int EffectPriority => -1;

        private bool _otherBoonNegatedDeath;

        public override void SetDefaults() {
            Item.DefaultToAccessory();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override bool PrePlayerKill(Player player, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            if (player.HasBuff<AnubisDeathNegateDebuff>() || player.HasBuff<AnubisBoonNegateDebuff>()) {
                return true;
            }
            else if (_otherBoonNegatedDeath) {
                _otherBoonNegatedDeath = false;

                return true;
            }

            player.AddBuff(ModContent.BuffType<AnubisDeathNegateDebuff>(), 60);
            player.statLife = player.statLifeMax2;
            Item.type = ItemID.None;
            Item.stack = 0;
            return false;
        }

        public override bool PrePlayerForceKill(Player player, PlayerDeathReason deathReason) {
            if (IsPotent && deathReason.SourceCustomReason == "Boon") {
                player.AddBuff(ModContent.BuffType<AnubisBoonNegateDebuff>(), 60);
                player.statLife = player.statLifeMax2;
                return false;
            }

            return true;
        }

        public override void PlayerDeathNegated(Player player) {
            _otherBoonNegatedDeath = true;
        }
    }
}