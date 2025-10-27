using System.Linq;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;


namespace LivingWorldMod.Content.Villages.HarpyVillage.Biomes;

/// <summary>
///     "Biome" that surrounds a given harpy shrine.
/// </summary>
public class HarpyVillageBiome : ModBiome {
    public override int Music => MusicLoader.GetMusicSlot(Mod, LWM.MusicPath + $"Village/Harpy{(Main.dayTime ? "Day" : "Night")}");

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Harpy Village");
    }

    public override bool IsBiomeActive(Player player) =>
        LWMUtils.GetAllEntityOfType<VillageShrineEntity>().Any(entity => entity.shrineType == VillagerType.Harpy && entity.villageZone.ContainsPoint(player.Center));
}