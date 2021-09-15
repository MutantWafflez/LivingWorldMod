using LivingWorldMod.Content.Tiles.Interactables.VillageShrines;
using LivingWorldMod.Custom.Enums;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TileEntities.VillageShrines {

    public class HarpyShrineEntity : VillageShrineEntity {
        public override VillagerType VillageType => VillagerType.Harpy;

        public override int ShrineTileID => ModContent.TileType<HarpyShrineTile>();
    }
}