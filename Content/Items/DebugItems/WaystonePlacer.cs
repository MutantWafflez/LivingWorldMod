using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Enums;
using Terraria;
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

        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player) {
            if (player.altFunctionUse == 2 && player.itemAnimation == 10) {
                if (++Item.placeStyle > 4) {
                    Item.placeStyle = 0;
                }
                Main.NewText($"Now placing {(WaystoneType)Item.placeStyle} Waystones");
            }

            return base.UseItem(player);
        }
    }
}