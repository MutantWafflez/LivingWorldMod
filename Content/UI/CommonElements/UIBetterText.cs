using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.CommonElements {

    /// <summary>
    /// Slightly better version of UIText that will properly scale based on width specified in a
    /// different parameter rather than using InnerDimensions, and other better functionality.
    /// Mostly mimics vanilla's UIText though.
    /// </summary>
    public class UIBetterText : UIElement {

        /// <summary>
        /// If set to a value greater than zero, it will constrain the text to fix within the value
        /// set, in pixels. Replaces DynamicallyScaleDownToWidth in UIText.
        /// </summary>
        public float horizontalTextConstraint;

        /// <summary>
        /// Tells how far the text can be drawn before wrapping around to the next line. Measured in
        /// pixels. Only works if isWrapped is true.
        /// </summary>
        public float horizontalWrapConstraint;

        /// <summary>
        /// Whether or not this element is visible or not, basically meaning whether or not it will
        /// be drawn or not. Defaults to true.
        /// </summary>
        public bool isVisible = true;

        public object innerText = "";
        private float initialTextScale = 1f;
        private float dynamicTextScale = 1f;
        private Vector2 initialTextSize = Vector2.Zero;
        private Vector2 dynamicTextSize = Vector2.Zero;
        private Color color = Color.White;

        private bool isLarge;
        private bool isWrapped;

        private string visibleText;
        private string lastTextReference;

        public event Action OnInternalTextChange;

        public string Text => innerText.ToString();

        public float TextOriginX {
            get;
            set;
        }

        public float TextOriginY {
            get;
            set;
        }

        public float WrappedTextBottomPadding {
            get;
            set;
        }

        public bool IsWrapped {
            get => isWrapped;
            set {
                isWrapped = value;
                InternalSetText(Text, initialTextScale, isLarge);
            }
        }

        public Color TextColor {
            get => color;
            set => color = value;
        }

        public UIBetterText(string text = "", float textScale = 1f, bool large = false) {
            TextOriginX = 0.5f;
            TextOriginY = 0f;
            IsWrapped = false;
            WrappedTextBottomPadding = 20f;
            InternalSetText(text, textScale, large);
        }

        public UIBetterText(LocalizedText text, float textScale = 1f, bool large = false) {
            TextOriginX = 0.5f;
            TextOriginY = 0f;
            IsWrapped = false;
            WrappedTextBottomPadding = 20f;
            InternalSetText(text, textScale, large);
        }

        public override void Recalculate() {
            InternalSetText(Text, initialTextScale, isLarge);
            base.Recalculate();
        }

        public void SetText(string text, float scaledText = 0f, bool? large = null) {
            InternalSetText(text, scaledText == 0f ? initialTextScale : scaledText, large ?? isLarge);
        }

        public void SetText(LocalizedText text, float scaledText = 0f, bool? large = null) {
            InternalSetText(text, scaledText == 0f ? initialTextScale : scaledText, large ?? isLarge);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            if (!isVisible) {
                return;
            }

            base.DrawSelf(spriteBatch);
            VerifyTextState();

            CalculatedStyle innerDimensions = GetInnerDimensions();
            Vector2 pos = innerDimensions.Position();

            if (isLarge) {
                pos.Y -= 10f * dynamicTextScale;
            }
            else {
                pos.Y -= 2f * dynamicTextScale;
            }

            pos.X += (innerDimensions.Width - dynamicTextSize.X) * TextOriginX;
            pos.Y += (innerDimensions.Height - dynamicTextSize.Y) * TextOriginY;

            if (isLarge) {
                Utils.DrawBorderStringBig(spriteBatch, visibleText, pos, color, dynamicTextScale);
            }
            else {
                Utils.DrawBorderString(spriteBatch, visibleText, pos, color, dynamicTextScale);
            }
        }

        private void VerifyTextState() {
            if (!ReferenceEquals(lastTextReference, Text)) {
                InternalSetText(Text, initialTextScale, isLarge);
            }
        }

        private void InternalSetText(object text, float textScale, bool large) {
            DynamicSpriteFont dynamicSpriteFont = large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;

            innerText = text;
            isLarge = large;
            initialTextScale = textScale;
            dynamicTextScale = textScale;
            lastTextReference = Text;

            visibleText = IsWrapped ? dynamicSpriteFont.CreateWrappedText(lastTextReference, horizontalWrapConstraint) : lastTextReference;

            Vector2 stringSize = dynamicSpriteFont.MeasureString(visibleText);

            initialTextSize = !IsWrapped ? new Vector2(stringSize.X, isLarge ? 32f : 16f) * initialTextScale : new Vector2(stringSize.X, stringSize.Y + WrappedTextBottomPadding) * initialTextScale;

            if (horizontalTextConstraint > 0f && initialTextSize.X > horizontalTextConstraint) {
                dynamicTextScale *= horizontalTextConstraint / initialTextSize.X;
                dynamicTextSize = !IsWrapped ? new Vector2(stringSize.X, isLarge ? 32f : 16f) * dynamicTextScale : new Vector2(stringSize.X, stringSize.Y + WrappedTextBottomPadding) * dynamicTextScale;
            }
            else {
                dynamicTextScale = initialTextScale;
                dynamicTextSize = initialTextSize;
            }

            if (horizontalTextConstraint > 0f) {
                MaxWidth.Set(initialTextSize.X + PaddingLeft + PaddingRight, 0f);
                Width.Set(horizontalTextConstraint, 0f);
                Height = MaxHeight = new StyleDimension(dynamicTextSize.Y + PaddingTop + PaddingBottom, 0f);
            }
            else {
                Width = MaxWidth = new StyleDimension(initialTextSize.X + PaddingLeft + PaddingRight, 0f);
                Height = MaxHeight = new StyleDimension(initialTextSize.Y + PaddingTop + PaddingBottom, 0f);
            }

            OnInternalTextChange?.Invoke();
        }
    }
}