using LivingWorldMod.Globals.BaseTypes.Items;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;

public class NimbusJarItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.BirdCage);
        Item.rare = ItemRarityID.Blue;
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(gold: 1);
        Item.createTile = ModContent.TileType<NimbusJarTile>();
    }
}