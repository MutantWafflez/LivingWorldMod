using LivingWorldMod.Content.Items.Accessories.Boons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.StatusEffects.Debuffs.Boons {
    /// <summary>
    /// Debuff granted to the player when they negate the forced death
    /// caused by another boon using the potent effect of the <seealso cref="AnubisBoon"/>
    /// item.
    /// </summary>
    public class AnubisBoonNegateDebuff : BaseStatusEffect {
        public override void SetStaticDefaults() {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;

            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) {
            player.statLifeMax2 = 100;
            player.GetDamage(DamageClass.Generic) -= 0.5f;
        }
    }
}