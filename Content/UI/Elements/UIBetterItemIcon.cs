using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// A better version of Vanilla's UIItemIcon class. Uses the position to draw instead of the
    /// center. Very simple. I know.
    /// </summary>
    public class UIBetterItemIcon : UIElement {
        public readonly int context = ItemSlot.Context.InventoryItem;

        private Item displayedItem;

        public UIBetterItemIcon(Item displayedItem) {
            this.displayedItem = displayedItem;

            Width.Set(32f, 0f);
            Height.Set(32f, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            Main.DrawItemIcon(spriteBatch, displayedItem, GetDimensions().Position(), Color.White, 32f);
        }
    }
}