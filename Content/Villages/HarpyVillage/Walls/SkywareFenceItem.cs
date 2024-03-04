using LivingWorldMod.Globals.BaseTypes.Items;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Walls;

public class SkywareFenceItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 40;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.DirtWall);
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(copper: 25);
        Item.createWall = ModContent.WallType<SkywareFenceWall>();
    }
}