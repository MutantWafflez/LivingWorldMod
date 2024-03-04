using LivingWorldMod.Globals.BaseTypes.Items;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;

public class SkywareLoomItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.LivingLoom);
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(silver: 40);
        Item.createTile = ModContent.TileType<SkywareLoomTile>();
    }
}