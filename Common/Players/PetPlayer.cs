using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {

    /// <summary>
    /// ModPlayer class that handles all pets in the mod.
    /// </summary>
    public class PetPlayer : ModPlayer {
        public bool nimbusPet;

        public override void ResetEffects() {
            nimbusPet = false;
        }
    }
}