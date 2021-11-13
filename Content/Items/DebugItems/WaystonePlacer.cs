using LivingWorldMod.Content.Tiles.Interactables;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.DebugItems {
    /// <summary>
    /// Debug item that places a waystone, depending on what biome you are in.
    /// </summary>
    public class WaystonePlacer : DebugItem {

        public override string Texture => "Terraria/Images/Item_" + ItemID.AnkhCharm;

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DirtBlock);
            Item.maxStack = 1;
            Item.consumable = false;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.placeStyle = 0;
            Item.createTile = ModContent.TileType<WaystoneTile>();
        }
    }
}