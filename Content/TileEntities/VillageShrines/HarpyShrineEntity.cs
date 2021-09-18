using LivingWorldMod.Content.Tiles.Interactables.VillageShrines;
using LivingWorldMod.Custom.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TileEntities.VillageShrines {

    public class HarpyShrineEntity : VillageShrineEntity {
        public override VillagerType VillageType => VillagerType.Harpy;

        public override int VillageZoneDustType => DustID.BlueFairy;

        public override int ValidTileID => ModContent.TileType<HarpyShrineTile>();
    }
}