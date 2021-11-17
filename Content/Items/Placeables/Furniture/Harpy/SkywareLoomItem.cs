using LivingWorldMod.Content.Tiles.Furniture.Harpy;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeables.Furniture.Harpy {
    public class SkywareLoomItem : BaseItem {
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.LivingLoom);
            Item.placeStyle = 0;
            Item.value = Terraria.Item.buyPrice(silver: 40);
            Item.createTile = ModContent.TileType<SkywareLoomTile>();
        }
    }
}