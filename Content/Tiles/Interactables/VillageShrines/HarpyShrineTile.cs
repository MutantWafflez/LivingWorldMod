using LivingWorldMod.Content.Items.Placeables.Interactables;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles.Interactables.VillageShrines {

    public class HarpyShrineTile : VillageShrineTile {
        public override int ItemDropType => ModContent.ItemType<HarpyShrineItem>();
    }
}