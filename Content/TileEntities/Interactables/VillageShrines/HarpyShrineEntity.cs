using LivingWorldMod.Content.Tiles.Interactables.VillageShrines;
using LivingWorldMod.Custom.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TileEntities.Interactables.VillageShrines {

    public class HarpyShrineEntity : VillageShrineEntity {
        public override int ValidTileID => ModContent.TileType<HarpyShrineTile>();

        public override VillagerType VillageType => VillagerType.Harpy;

        public override int VillageZoneDustType => DustID.BlueFairy;

        public override float VillageRadius => 1360f;
    }
}