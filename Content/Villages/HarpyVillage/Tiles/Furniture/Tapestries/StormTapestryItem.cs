using LivingWorldMod.Content.Tiles.Furniture.Tapestries;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeables.Furniture.Tapestries;

public class StormTapestryItem : BaseItem {
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
        mod.AddContent(new TapestryTile(Name, Color.CadetBlue));

        return true;
    }
}