using LivingWorldMod.Globals.BaseTypes.Items;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;

public class SkywareAnvilItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.IronAnvil);
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(silver: 5);
        Item.createTile = ModContent.TileType<SkywareAnvilTile>();
    }
}