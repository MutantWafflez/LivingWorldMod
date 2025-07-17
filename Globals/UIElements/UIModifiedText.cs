using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

/// <summary>
///     Modification of UIText that will properly scale based on width specified in a
///     different parameter rather than using InnerDimensions, and other better functionality.
/// </summary>
public class UIModifiedText : UIElement {
    /// <summary>
    ///     If set to a value greater than zero, it will constrain the text to fix within the value
    ///     set, in pixels. Replaces DynamicallyScaleDownToWidth in UIText.
    /// </summary>
    public float horizontalTextConstraint;

    /// <summary>
    ///     Tells how far the text can be drawn before wrapping around to the next line. Measured in
    ///     pixels. Only works if isWrapped is true.
    /// </summary>
    public float horizontalWrapConstraint;

    private float _initialTextScale = 1f;
    private float _dynamicTextScale = 1f;
    private Vector2 _initialTextSize = Vector2.Zero;
    private Vector2 _dynamicTextSize = Vector2.Zero;

    private bool _isLarge;
    private bool _isWrapped;

    private string _visibleText;
    private string _lastTextReference;

    public string Text {
        get;
        private set;
    } = "";

    public float TextOriginX {
        get;
        set;
    } = 0.5f;

    public float TextOriginY {
        get;
        set;
    } = 0f;

    public float WrappedTextBottomPadding {
        get;
        set;
    } = 20f;

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

    public UIModifiedText(string text = "", float textScale = 1f, bool large = false) {
        IsWrapped = false;
        InternalSetText(text, textScale, large);
    }

    public UIModifiedText(LocalizedText text, float textScale = 1f, bool large = false) : this(text.Value, textScale, large) { }

    public override void Recalculate() {
        InternalSetText(Text, _initialTextScale, _isLarge);
        base.Recalculate();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);

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

    public void SetText(string text, float scaledText = 0f, bool? large = null) {
        InternalSetText(text, scaledText == 0f ? _initialTextScale : scaledText, large ?? _isLarge);
    }

    public void SetText(LocalizedText text, float scaledText = 0f, bool? large = null) => SetText(text.Value, scaledText, large);

    private void InternalSetText(string text, float textScale, bool large) {
        DynamicSpriteFont dynamicSpriteFont = large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;

        Text = text;
        _isLarge = large;
        _initialTextScale = textScale;
        _dynamicTextScale = textScale;
        _lastTextReference = Text;

        _visibleText = IsWrapped ? dynamicSpriteFont.CreateWrappedText(_lastTextReference, horizontalWrapConstraint) : _lastTextReference;

        Vector2 stringSize = dynamicSpriteFont.MeasureString(_visibleText);

        _initialTextSize = !IsWrapped ? new Vector2(stringSize.X, _isLarge ? 32f : 16f) * _initialTextScale : new Vector2(stringSize.X, stringSize.Y + WrappedTextBottomPadding) * _initialTextScale;

        if (horizontalTextConstraint > 0f && _initialTextSize.X > horizontalTextConstraint) {
            _dynamicTextScale *= horizontalTextConstraint / _initialTextSize.X;
            _dynamicTextSize = !IsWrapped
                ? new Vector2(stringSize.X, _isLarge ? 32f : 16f) * _dynamicTextScale
                : new Vector2(stringSize.X, stringSize.Y + WrappedTextBottomPadding) * _dynamicTextScale;
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