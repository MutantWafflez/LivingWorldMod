using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace LivingWorldMod.Globals.UIElements;

/// <summary>
///     Rewrite of <see cref="UIText" /> that has more functionality and operates more smoothly.
/// </summary>
public class UIModifiedText : UIElement {
    public float textOriginX = 0.5f;
    public float textOriginY;
    private readonly bool _textInitialized;

    private string _desiredText;
    private Vector2 _desiredTextScale;
    private float? _wrapConstraint;
    private float? _horizontalTextConstraint;

    /// <summary>
    ///     If set to a non-zero value, it will constrain the text to fix within the value
    ///     set, in pixels.
    /// </summary>
    public float? HorizontalTextConstraint {
        get => _horizontalTextConstraint;
        set {
            _horizontalTextConstraint = value;

            TryRecalculate();
        }
    }

    /// <summary>
    ///     If a non-zero value, denotes how far the text can be drawn before wrapping around to the next line.
    /// </summary>
    public float? WrapConstraint {
        get => _wrapConstraint;
        set {
            _wrapConstraint = value;

            TryRecalculate();
        }
    }

    public Vector2 DesiredTextScale {
        get => _desiredTextScale;
        set {
            _desiredTextScale = value;

            TryRecalculate();
        }
    }

    public Vector2 TextScale {
        get;
        private set;
    }

    public Vector2 TextSize {
        get;
        private set;
    }

    public string DesiredText {
        get => _desiredText;
        set {
            _desiredText = value;

            TryRecalculate();
        }
    }

    public string Text {
        get;
        private set;
    }

    public Color TextColor {
        get;
        init;
    }

    public DynamicSpriteFont TextFont {
        get;
    }

    public UIModifiedText(string text, Vector2 textScale, bool large = false) {
        DesiredText = text;
        DesiredTextScale = textScale;
        TextFont = large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;

        RecalculateText();
        _textInitialized = true;
    }

    public UIModifiedText(string text = "", float textScale = 1f, bool large = false) : this(text, new Vector2(textScale), large) { }

    public UIModifiedText(LocalizedText text, float textScale = 1f, bool large = false) : this(text.Value, textScale, large) { }

    public UIModifiedText(LocalizedText text, Vector2 textScale, bool large = false) : this(text.Value, textScale, large) { }

    public override void Recalculate() {
        RecalculateText();
        base.Recalculate();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);

        CalculatedStyle innerDimensions = GetInnerDimensions();
        Vector2 pos = innerDimensions.Position();

        pos.X += (innerDimensions.Width - TextSize.X) * textOriginX;
        pos.Y += (innerDimensions.Height - TextSize.Y) * textOriginY - (TextFont == FontAssets.DeathText.Value ? 10f : 2f) * TextScale.Y;

        Vector2 origin = new Vector2(0f, 0f) * TextFont.MeasureString(Text);
        TextSnippet[] textSnippets = ChatManager.ParseMessage(Text, TextColor).ToArray();
        ChatManager.ConvertNormalSnippets(textSnippets);

        ChatManager.DrawColorCodedStringShadow(spriteBatch, TextFont, textSnippets, pos, Color.Black * (TextColor.A / (float)byte.MaxValue), 0f, origin, TextScale);
        ChatManager.DrawColorCodedString(spriteBatch, TextFont, textSnippets, pos, Color.White, 0f, origin, TextScale, out _, -1f);
    }

    private void TryRecalculate() {
        if (!_textInitialized) {
            return;
        }

        Recalculate();
    }

    private void RecalculateText() {
        TextScale = DesiredTextScale;
        Text = WrapConstraint is { } wrapConstraint ? TextFont.CreateWrappedText(DesiredText, wrapConstraint) : DesiredText;

        Vector2 textSize = ChatManager.GetStringSize(TextFont, Text, TextScale);
        if (HorizontalTextConstraint is { } horizontalConstraint && textSize.X * TextScale.X > horizontalConstraint) {
            Vector2 textScale = TextScale;
            textScale.X *= horizontalConstraint / (textSize.X * TextScale.X);

            TextScale = textScale;
            TextSize = textSize * TextScale;

            Width = StyleDimension.FromPixels(horizontalConstraint);
        }
        else {
            TextSize = textSize;

            Width = StyleDimension.FromPixels(textSize.X + PaddingLeft + PaddingRight);
        }

        Height = StyleDimension.FromPixels(textSize.Y + PaddingTop + PaddingBottom);
    }
}