using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.StatusEffects.Debuffs {
    /// <summary>
    /// Debuff applied by the Curse of Pacifism in the Pyramid Dungeon.
    /// </summary>
    //TODO: Get proper sprite for debuff
    public class PacifistPlight : BaseStatusEffect {
        public override void SetStaticDefaults() {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) {
            player.GetDamage(DamageClass.Generic) -= 0.34f;
            player.endurance /= 3f;
            player.statDefense /= 3;
        }
    }
}