using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.Tiles;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture.Tapestries;

public class SunTapestryItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.BlueBanner);
        Item.rare = ItemRarityID.Blue;
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(silver: 20);
        Item.createTile = Mod.Find<ModTile>(Name.Replace("Item", "Tile")).Type;
    }

    public override bool IsLoadingEnabled(Mod mod) {
        mod.AddContent(new TapestryTile(this, Color.DarkSlateGray));

        return true;
    }
}