using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.CommonElements {
    /// <summary>
    /// A better version of Vanilla's UIItemIcon class. Can use position or the center to draw from,
    /// and has hover tooltip functionality.
    /// </summary>
    public class UIBetterItemIcon : UIElement {
        public readonly int context = ItemSlot.Context.InventoryItem;

        /// <summary>
        /// Whether or not this element is currently visible, which is to say, whether or not it
        /// will be drawn. Defaults to true.
        /// </summary>
        public bool isVisible = true;

        private Item _displayedItem;
        private float _sizeLimit;
        private bool _drawFromCenter;

        public UIBetterItemIcon(Item displayedItem, float sizeLimit, bool drawFromCenter) {
            _displayedItem = displayedItem;
            _sizeLimit = sizeLimit;
            _drawFromCenter = drawFromCenter;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            if (!isVisible) {
                return;
            }

            //Adapted Vanilla Code
            Main.instance.LoadItem(_displayedItem.type);

            Texture2D itemTexture = TextureAssets.Item[_displayedItem.type].Value;
            Rectangle itemAnimFrame = Main.itemAnimations[_displayedItem.type] == null ? itemTexture.Frame() : Main.itemAnimations[_displayedItem.type].GetFrame(itemTexture);

            Color currentColor = Color.White;
            float itemLightScale = 1f;
            float sizeConstraint = 1f;

            ItemSlot.GetItemLight(ref currentColor, ref itemLightScale, _displayedItem);
            sizeConstraint *= itemLightScale;

            if (itemAnimFrame.Width > _sizeLimit || itemAnimFrame.Height > _sizeLimit) {
                sizeConstraint = itemAnimFrame.Width <= itemAnimFrame.Height ? _sizeLimit / itemAnimFrame.Height : _sizeLimit / itemAnimFrame.Width;
            }

            sizeConstraint *= _displayedItem.scale;

            spriteBatch.Draw(itemTexture,
                _drawFromCenter ? GetDimensions().Center() : GetDimensions().Position(),
                itemAnimFrame,
                currentColor,
                0f,
                _drawFromCenter ? new Vector2(itemAnimFrame.Width / 2f, itemAnimFrame.Height / 2f) : default,
                sizeConstraint,
                SpriteEffects.None,
                0f);

            //Non-vanilla code
            if (ContainsPoint(Main.MouseScreen)) {
                ItemSlot.MouseHover(ref _displayedItem, context);
            }
        }

        public void SetItem(Item newItem) {
            _displayedItem = newItem;
            Recalculate();
        }
    }
}