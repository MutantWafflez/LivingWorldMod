using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.CommonElements {
    /// <summary>
    /// Slightly better version of UIText that will properly scale based on width specified in a
    /// different parameter rather than using InnerDimensions, and other better functionality.
    /// Mostly mimics vanilla's UIText though.
    /// </summary>
    public class UIBetterText : UIElement {
        public string Text => _innerText is ModTranslation translation ? translation.GetTranslation(Language.ActiveCulture) : _innerText.ToString();

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
            get => _isWrapped;
            set {
                _isWrapped = value;
                InternalSetText(Text, _initialTextScale, _isLarge);
            }
        }

        public Color TextColor {
            get;
            set;
        } = Color.White;

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

        private object _innerText = "";

        private float _initialTextScale = 1f;
        private float _dynamicTextScale = 1f;
        private Vector2 _initialTextSize = Vector2.Zero;
        private Vector2 _dynamicTextSize = Vector2.Zero;

        private bool _isLarge;
        private bool _isWrapped;

        private string _visibleText;
        private string _lastTextReference;

        public UIBetterText(string text = "", float textScale = 1f, bool large = false) {
            TextOriginX = 0.5f;
            TextOriginY = 0f;
            IsWrapped = false;
            WrappedTextBottomPadding = 20f;
            InternalSetText(text, textScale, large);
        }

        public UIBetterText(ModTranslation text, float textScale = 1f, bool large = false) {
            TextOriginX = 0.5f;
            TextOriginY = 0f;
            IsWrapped = false;
            WrappedTextBottomPadding = 20f;
            InternalSetText(text, textScale, large);
        }

        public override void Recalculate() {
            InternalSetText(Text, _initialTextScale, _isLarge);
            base.Recalculate();
        }

        public void SetText(string text, float scaledText = 0f, bool? large = null) {
            InternalSetText(text, scaledText == 0f ? _initialTextScale : scaledText, large ?? _isLarge);
        }

        public void SetText(ModTranslation text, float scaledText = 0f, bool? large = null) {
            InternalSetText(text, scaledText == 0f ? _initialTextScale : scaledText, large ?? _isLarge);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            if (!isVisible) {
                return;
            }

            base.DrawSelf(spriteBatch);
            VerifyTextState();

            CalculatedStyle innerDimensions = GetInnerDimensions();
            Vector2 pos = innerDimensions.Position();

            if (_isLarge) {
                pos.Y -= 10f * _dynamicTextScale;
            }
            else {
                pos.Y -= 2f * _dynamicTextScale;
            }

            pos.X += (innerDimensions.Width - _dynamicTextSize.X) * TextOriginX;
            pos.Y += (innerDimensions.Height - _dynamicTextSize.Y) * TextOriginY;

            if (_isLarge) {
                Utils.DrawBorderStringBig(spriteBatch, _visibleText, pos, TextColor, _dynamicTextScale);
            }
            else {
                Utils.DrawBorderString(spriteBatch, _visibleText, pos, TextColor, _dynamicTextScale);
            }
        }

        private void VerifyTextState() {
            if (!ReferenceEquals(_lastTextReference, Text)) {
                InternalSetText(Text, _initialTextScale, _isLarge);
            }
        }

        private void InternalSetText(object text, float textScale, bool large) {
            DynamicSpriteFont dynamicSpriteFont = large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;

            _innerText = text;
            _isLarge = large;
            _initialTextScale = textScale;
            _dynamicTextScale = textScale;
            _lastTextReference = Text;

            _visibleText = IsWrapped ? dynamicSpriteFont.CreateWrappedText(_lastTextReference, horizontalWrapConstraint) : _lastTextReference;

            Vector2 stringSize = dynamicSpriteFont.MeasureString(_visibleText);

            _initialTextSize = !IsWrapped ? new Vector2(stringSize.X, _isLarge ? 32f : 16f) * _initialTextScale : new Vector2(stringSize.X, stringSize.Y + WrappedTextBottomPadding) * _initialTextScale;

            if (horizontalTextConstraint > 0f && _initialTextSize.X > horizontalTextConstraint) {
                _dynamicTextScale *= horizontalTextConstraint / _initialTextSize.X;
                _dynamicTextSize = !IsWrapped ? new Vector2(stringSize.X, _isLarge ? 32f : 16f) * _dynamicTextScale : new Vector2(stringSize.X, stringSize.Y + WrappedTextBottomPadding) * _dynamicTextScale;
            }
            else {
                _dynamicTextScale = _initialTextScale;
                _dynamicTextSize = _initialTextSize;
            }

            if (horizontalTextConstraint > 0f) {
                MaxWidth.Set(_initialTextSize.X + PaddingLeft + PaddingRight, 0f);
                Width.Set(horizontalTextConstraint, 0f);
                Height = MaxHeight = new StyleDimension(_dynamicTextSize.Y + PaddingTop + PaddingBottom, 0f);
            }
            else {
                Width = MaxWidth = new StyleDimension(_initialTextSize.X + PaddingLeft + PaddingRight, 0f);
                Height = MaxHeight = new StyleDimension(_initialTextSize.Y + PaddingTop + PaddingBottom, 0f);
            }

            OnInternalTextChange?.Invoke();
        }

        public event Action OnInternalTextChange;
    }
}