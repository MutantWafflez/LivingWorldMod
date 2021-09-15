using LivingWorldMod.Custom.Enums;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {

    /// <summary>
    /// ModPlayer class that handles biome related tasks, such as whether or not a player is within
    /// a specified biome, for example.
    /// </summary>
    public class BiomePlayer : ModPlayer {

        /// <summary>
        /// The current village biome this player is in, if applicable. Null designates that the
        /// player is not within ANY village biome.
        /// </summary>
        public VillagerType? currentVillageBiome;

        public override void ResetEffects() {
            currentVillageBiome = null;
        }
    }
}