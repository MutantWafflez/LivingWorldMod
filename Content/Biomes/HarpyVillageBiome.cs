using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using System.Linq;
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

        public override bool IsBiomeActive(Player player) => TileEntityUtils.GetAllEntityOfType<VillageShrineEntity>().Any(entity => entity.shrineType == VillagerType.Harpy && entity.villageZone.ContainsPoint(player.Center));
    }
}