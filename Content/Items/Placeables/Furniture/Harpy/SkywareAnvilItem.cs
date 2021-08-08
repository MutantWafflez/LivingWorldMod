using LivingWorldMod.Content.Tiles.Furniture.Harpy;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeables.Furniture.Harpy {

    public class SkywareAnvilItem : BaseItem {

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.IronAnvil);
            Item.placeStyle = 0;
            Item.value = Terraria.Item.buyPrice(silver: 5);
            Item.createTile = ModContent.TileType<SkywareAnvilTile>();
        }
    }
}