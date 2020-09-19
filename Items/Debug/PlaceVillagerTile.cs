using LivingWorldMod.Tiles.WorldGen;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Debug
{
    public class PlaceVillagerTile : DebugItem
    {
        public override string Texture => "Terraria/UI/DisplaySlots_5";
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.DirtBlock);
            item.createTile = ModContent.TileType<VillagerHomeTile>();
        }
    }
}
