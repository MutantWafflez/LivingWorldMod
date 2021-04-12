using LivingWorldMod.Content.Tiles.WorldGen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeable.Herbs {

    public class SkyBudSeeds : ModItem {

        public override void SetStaticDefaults() {
            Tooltip.SetDefault("Can only be planted on clouds");
        }

        public override void SetDefaults() {
            item.CloneDefaults(ItemID.DaybloomSeeds);
            item.placeStyle = 0;
            item.value = Item.sellPrice(silver: 2);
            item.createTile = ModContent.TileType<SkyBudHerb>();
            item.material = true;
        }
    }
}