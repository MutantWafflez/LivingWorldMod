using LivingWorldMod.Tiles.Furniture.Paintings;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Placeable.Paintings
{
    public class OneStarTileItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("One Star");
            Tooltip.SetDefault("'R. Oaken'");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.TheMerchant);
            item.value = Item.buyPrice(gold: 1);
            item.createTile = ModContent.TileType<OneStarTile>();
            item.placeStyle = 0;
        }
    }
}
