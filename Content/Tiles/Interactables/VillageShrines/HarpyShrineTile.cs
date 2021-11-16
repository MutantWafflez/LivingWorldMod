using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.Items.Placeables.Interactables;
using LivingWorldMod.Content.TileEntities.Interactables.VillageShrines;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles.Interactables.VillageShrines {

    public class HarpyShrineTile : VillageShrineTile {
        public override int ItemDropType => ModContent.ItemType<HarpyShrineItem>();

        public override VillageShrineEntity ShrineEntity => TileEntitySystem.GetBaseEntityInstance<VillageShrineEntity>();
    }
}