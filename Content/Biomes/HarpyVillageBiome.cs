using LivingWorldMod.Common.Players;
using LivingWorldMod.Custom.Enums;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Biomes {
    /// <summary>
    /// "Biome" that surrounds a given harpy shrine.
    /// </summary>
    public class HarpyVillageBiome : ModBiome {
        public override int Music => MusicLoader.GetMusicSlot(Mod, LivingWorldMod.LWMMusicPath + $"Village/Harpy{(Main.dayTime ? "Day" : "Night")}");

        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Harpy Village");
        }

        public override bool IsBiomeActive(Player player) => player.GetModPlayer<BiomePlayer>().currentVillageBiome == VillagerType.Harpy;
    }
}