using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// A better version of Vanilla's UIItemIcon class. Uses the position to draw instead of the
    /// center, which causes offset upon reloading the UI this element is in.
    /// </summary>
    public class UIBetterItemIcon : UIElement {
        private Item item;
        private bool blackedOut;

        public UIBetterItemIcon(Item item, bool blackedOut) {
            this.item = item;
            Width.Set(32f, 0f);
            Height.Set(32f, 0f);
            this.blackedOut = blackedOut;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            Main.DrawItemIcon(spriteBatch, item, GetDimensions().Position(), blackedOut ? Color.Black : Color.White, 32f);
        }
    }
}