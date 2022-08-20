using LivingWorldMod.Content.Items.Accessories.Boons;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.StatusEffects.Debuffs.Boons {
    /// <summary>
    /// Debuff granted to the player when they negate their own death with
    /// the <seealso cref="AnubisBoon"/> item.
    /// </summary>
    public class AnubisDeathNegateDebuff : BaseStatusEffect {
        public override void SetStaticDefaults() {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;

            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) {
            player.statLifeMax2 /= 2;
        }
    }
}