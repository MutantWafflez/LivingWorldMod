using LivingWorldMod.Content.Tiles.Interactables;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.DebugItems {
    /// <summary>
    /// Debug item that can place Crypt Door objects.
    /// </summary>
    public class PyramidDoorItem : DebugItem {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SandstoneDoor;

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DirtBlock);
            Item.maxStack = 1;
            Item.consumable = false;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.placeStyle = 0;
            Item.createTile = ModContent.TileType<PyramidDoorTile>();
        }
    }
}