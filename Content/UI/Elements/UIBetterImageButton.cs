using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// A better version of vanilla's UIImage Button, with much more functionality than the vanilla
    /// counterpart has, for a larger range of uses.
    /// </summary>
    public class UIBetterImageButton : UIElement {
        public UIBetterText buttonText;

        public float textSize;

        /// <summary>
        /// Whether or not this element is currently visible, which is to say, whether or not it
        /// will be drawn. Defaults to true.
        /// </summary>
        public bool isVisible = true;

        private string text;

        private Asset<Texture2D> buttonTexture;

        private Asset<Texture2D> borderTexture;

        private float activeVisibility = 1f;

        private float inactiveVisibility = 0.4f;

        /// <summary>
        /// Simple action/event that triggers after every frame while the mouse is currently
        /// hovering over this element.
        /// </summary>
        public event Action WhileHovering;

        //Remember to use ImmediateLoad request mode if you request the texture in the parameter!
        public UIBetterImageButton(Asset<Texture2D> buttonTexture, string text = null, float textSize = 1f) {
            this.buttonTexture = buttonTexture;
            this.text = text;
            this.textSize = textSize;
            Width.Set(buttonTexture.Value.Width, 0f);
            Height.Set(buttonTexture.Value.Height, 0f);
        }

        public override void OnInitialize() {
            if (text is null) {
                return;
            }

            buttonText = new UIBetterText(text, textSize) {
                HAlign = 0.5f,
                VAlign = 0.5f,
                horizontalTextConstraint = buttonTexture.Value.Width
            };

            Append(buttonText);
        }

        public void SetHoverImage(Asset<Texture2D> texture) => borderTexture = texture;

        public void SetImage(Asset<Texture2D> texture) {
            buttonTexture = texture;
            Width.Set(buttonTexture.Width(), 0f);
            Height.Set(buttonTexture.Height(), 0f);

            if (buttonText is not null) {
                buttonText.horizontalTextConstraint = buttonTexture.Width();
            }

            RecalculateChildren();
        }

        public void SetText(string text) {
            this.text = text;
            RecalculateChildren();
        }

        public override void MouseOver(UIMouseEvent evt) {
            if (!isVisible) {
                return;
            }

            base.MouseOver(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public void SetVisibility(float whenActive, float whenInactive) {
            activeVisibility = MathHelper.Clamp(whenActive, 0.0f, 1f);
            inactiveVisibility = MathHelper.Clamp(whenInactive, 0.0f, 1f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            if (buttonText is not null) {
                buttonText.isVisible = isVisible;
            }

            if (!isVisible) {
                return;
            }

            CalculatedStyle dimensions = GetDimensions();
            spriteBatch.Draw(buttonTexture.Value, dimensions.Position(), Color.White * (IsMouseHovering ? activeVisibility : inactiveVisibility));
            if (!IsMouseHovering) {
                return;
            }

            WhileHovering?.Invoke();

            if (borderTexture != null) {
                spriteBatch.Draw(borderTexture.Value, dimensions.Position(), Color.White);
            }
        }
    }
}