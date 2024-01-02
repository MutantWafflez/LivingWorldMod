using LivingWorldMod.Content.Tiles.Furniture.Harpy;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeables.Furniture.Harpy;

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