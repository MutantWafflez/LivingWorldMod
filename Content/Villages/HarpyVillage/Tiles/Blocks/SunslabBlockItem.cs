using LivingWorldMod.Globals.BaseTypes.Items;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Blocks;

public class SunslabBlockItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 50;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.DirtBlock);
        Item.value = Item.buyPrice(silver: 1);
        Item.placeStyle = 0;
        Item.createTile = ModContent.TileType<SunslabBlockTile>();
    }
}