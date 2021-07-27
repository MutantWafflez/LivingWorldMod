using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// A better version of Vanilla's UIItemIcon class. Uses the position to draw instead of the
    /// center, and displays the tooltip when hovering over the icon, as well.
    /// </summary>
    public class UIBetterItemIcon : UIElement {
        public readonly int context = ItemSlot.Context.InventoryItem;

        private Item displayedItem;
        private float sizeLimit;

        public UIBetterItemIcon(Item displayedItem, float sizeLimit) {
            this.displayedItem = displayedItem;
            this.sizeLimit = sizeLimit;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            //Adapted Vanilla Code
            Main.instance.LoadItem(displayedItem.type);

            Texture2D itemTexture = TextureAssets.Item[displayedItem.type].Value;
            Rectangle itemAnimFrame = (Main.itemAnimations[displayedItem.type] == null) ? itemTexture.Frame() : Main.itemAnimations[displayedItem.type].GetFrame(itemTexture);

            Vector2 drawPos = GetDimensions().Position();

            Color currentColor = Color.White;
            float itemLightScale = 1f;
            float sizeConstraint = 1f;

            ItemSlot.GetItemLight(ref currentColor, ref itemLightScale, displayedItem);
            sizeConstraint *= itemLightScale;

            if (itemAnimFrame.Width > sizeLimit || itemAnimFrame.Height > sizeLimit) {
                sizeConstraint = itemAnimFrame.Width <= itemAnimFrame.Height ? sizeLimit / itemAnimFrame.Height : sizeLimit / itemAnimFrame.Width;
            }

            sizeConstraint *= displayedItem.scale;

            spriteBatch.Draw(itemTexture, drawPos, itemAnimFrame, currentColor, 0f, default, sizeConstraint, SpriteEffects.None, 0f);

            //Non-vanilla code
            if (ContainsPoint(Main.MouseScreen)) {
                ItemSlot.MouseHover(ref displayedItem, context);
            }
        }
    }
}