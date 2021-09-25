using LivingWorldMod.Content.Tiles.Interactables.VillageShrines;
using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TileEntities.VillageShrines {

    public class HarpyShrineEntity : VillageShrineEntity {

        /// <summary>
        /// Whether or not this entity is the entity that spawns within the original Harpy Village
        /// generated upon world creation.
        /// </summary>
        public bool isOriginalVillageEntity;

        public override int ValidTileID => ModContent.TileType<HarpyShrineTile>();

        public override VillagerType VillageType => VillagerType.Harpy;

        public override int VillageZoneDustType => DustID.BlueFairy;

        public override Vector2 VillageOriginDisplacement => isOriginalVillageEntity ? new Vector2(0f, base.VillageRadius / 2f) : base.VillageOriginDisplacement;
    }
}