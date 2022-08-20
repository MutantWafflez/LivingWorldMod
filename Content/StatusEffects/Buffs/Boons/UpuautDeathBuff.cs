using LivingWorldMod.Content.Items.Accessories.Boons;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.StatusEffects.Buffs.Boons {
    /// <summary>
    /// In-game, called the "Blazy of Glory", triggered when you die using the
    /// <seealso cref="UpuautBoon"/> item.
    /// </summary>
    public class UpuautDeathBuff : BaseStatusEffect {
        public override void Update(Player player, ref int buffIndex) {
            player.statLife = 1;
            player.GetCritChance(DamageClass.Generic) = 100;
            player.GetDamage(DamageClass.Generic) += 0.2f;
            player.immune = true;
        }
    }
}