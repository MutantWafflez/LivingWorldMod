using LivingWorldMod.Content.Tiles.Furniture.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeable.Misc
{
    public class SkyBudPlanterBoxItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sky Bud Planter Box");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.DayBloomPlanterBox);
            item.value = Item.buyPrice(silver: 1);
            item.placeStyle = 0;
            item.createTile = ModContent.TileType<SkyBudPlanterBox>();
        }
    }
}