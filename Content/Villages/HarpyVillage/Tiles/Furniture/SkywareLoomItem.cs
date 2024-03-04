using LivingWorldMod.Content.Tiles.Furniture.Harpy;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeables.Furniture.Harpy;

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