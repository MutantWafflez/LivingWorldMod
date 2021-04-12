using LivingWorldMod.Content.Tiles.Furniture.Paintings;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeable.Paintings {

    public class SkyBustTileItem : ModItem {

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sky Bust");
            Tooltip.SetDefault("'R. Oaken'");
        }

        public override void SetDefaults() {
            item.CloneDefaults(ItemID.TheMerchant);
            item.value = Item.buyPrice(gold: 1);
            item.createTile = ModContent.TileType<SkyBustTile>();
            item.placeStyle = 0;
        }
    }
}