using LivingWorldMod.Globals.BaseTypes.Items;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;

public class HarpyShrineItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.maxStack = 99;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.rare = ItemRarityID.Orange;
        Item.consumable = true;
        Item.placeStyle = 0;
        Item.createTile = ModContent.TileType<VillageShrineTile>();
    }
}