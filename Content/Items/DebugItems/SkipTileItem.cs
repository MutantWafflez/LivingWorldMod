#if DEBUG

using LivingWorldMod.Content.Tiles.DebugTiles;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.DebugItems {

    public class SkipTileItem : BaseItem {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TeamBlockWhite;

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.TeamBlockWhite);
            Item.maxStack = 1;
            Item.consumable = false;
            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.rare = ItemRarityID.Quest;
            Item.createTile = ModContent.TileType<SkipTile>();
        }
    }
}

#endif