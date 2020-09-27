using LivingWorldMod.Tiles.Furniture.Paintings;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Placeable.Paintings
{
    public class SkyBustTileItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sky Bust");
            Tooltip.SetDefault("'R. Oaken'");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.TheMerchant);
            item.createTile = ModContent.TileType<SkyBustTile>();
            item.placeStyle = 0;
        }
    }
}
