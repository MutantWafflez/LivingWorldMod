using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

/// <summary>
///     A better version of vanilla's UIImage Button, with much more functionality than the vanilla
///     counterpart has, for a larger range of uses.
/// </summary>
public class UIBetterImageButton : UIElement {
    public UIModifiedText buttonText;

    public float textSize;

    /// <summary>
    ///     Whether or not, while the mousing is hovering over this element, the player can use an
    ///     item (mouseInterface = true). Defaults to true.
    /// </summary>
    public bool preventItemUsageWhileHovering = true;

    private object _text;

    private Asset<Texture2D> _buttonTexture;

    private Asset<Texture2D> _borderTexture;

    private float _activeVisibility = 1f;

    private float _inactiveVisibility = 0.4f;

    public UIBetterImageButton(Asset<Texture2D> buttonTexture, LocalizedText text = null, float textSize = 1f) {
        _buttonTexture = buttonTexture;
        _text = text;
        this.textSize = textSize;
        Width.Set(buttonTexture.Value.Width, 0f);
        Height.Set(buttonTexture.Value.Height, 0f);

        switch (_text) {
            case null:
                return;
            case LocalizedText translation:
                buttonText = new UIModifiedText(translation, textSize) { HAlign = 0.5f, VAlign = 0.5f, HorizontalTextConstraint = GetDimensions().Width, IgnoresMouseInteraction = true };
                break;
            default:
                buttonText = new UIModifiedText(_text as string, textSize) { HAlign = 0.5f, VAlign = 0.5f, HorizontalTextConstraint = GetDimensions().Width, IgnoresMouseInteraction = true };
                break;
        }

        Append(buttonText);

        OnMouseOver += MousedOverElement;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        bool isHovering = ContainsPoint(Main.MouseScreen);

        CalculatedStyle dimensions = GetDimensions();
        spriteBatch.Draw(_buttonTexture.Value, dimensions.Position(), Color.White * (isHovering ? _activeVisibility : _inactiveVisibility));

        //Hovering functionality
        if (!isHovering) {
            return;
        }

        WhileHovering?.Invoke();
        if (preventItemUsageWhileHovering) {
            Main.LocalPlayer.mouseInterface = true;
        }

        if (_borderTexture != null) {
            spriteBatch.Draw(_borderTexture.Value, dimensions.Position(), Color.White);
        }
    }

    public void SetHoverImage(Asset<Texture2D> texture) => _borderTexture = texture;

    public void SetImage(Asset<Texture2D> texture) {
        _buttonTexture = texture;
        Width.Set(_buttonTexture.Width(), 0f);
        Height.Set(_buttonTexture.Height(), 0f);

        if (buttonText is not null) {
            buttonText.HorizontalTextConstraint = _buttonTexture.Width();
        }

        RecalculateChildren();
    }

    public void SetText(string text) {
        _text = text;
        RecalculateChildren();
    }

    public void SetText(LocalizedText text) {
        _text = text;
        RecalculateChildren();
    }

    public void SetVisibility(float whenActive, float whenInactive) {
        _activeVisibility = MathHelper.Clamp(whenActive, 0.0f, 1f);
        _inactiveVisibility = MathHelper.Clamp(whenInactive, 0.0f, 1f);
    }

    private void MousedOverElement(UIMouseEvent evt, UIElement listeningElement) {
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    /// <summary>
    ///     Simple action/event that triggers after every frame while the mouse is currently
    ///     hovering over this element.
    /// </summary>
    public event Action WhileHovering;
}