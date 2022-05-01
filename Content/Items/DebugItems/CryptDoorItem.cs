using LivingWorldMod.Content.Tiles.Interactables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.DebugItems {
    /// <summary>
    /// Debug item that can place Crypt Door objects.
    /// </summary>
    public class CryptDoorItem : DebugItem {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SandstoneDoor;

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DirtBlock);
            Item.maxStack = 1;
            Item.consumable = false;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.placeStyle = 0;
            Item.createTile = ModContent.TileType<CryptDoorTile>();
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player) {
            if (player.altFunctionUse == 2 && player.itemAnimation == 10) {
                if (++Item.placeStyle > 1) {
                    Item.placeStyle = 0;
                }
                Main.NewText($"Now placing Crypt Door Tier {Item.placeStyle + 1}'s");
            }

            return base.UseItem(player);
        }
    }
}