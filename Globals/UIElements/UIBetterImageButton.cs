using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

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

    /// <summary>
    /// Whether or not, while the mousing is hovering over this element, the player can use an
    /// item (mouseInterface = true). Defaults to true.
    /// </summary>
    public bool preventItemUsageWhileHovering = true;

    private object _text;

    private Asset<Texture2D> _buttonTexture;

    private Asset<Texture2D> _borderTexture;

    private float _activeVisibility = 1f;

    private float _inactiveVisibility = 0.4f;

    public UIBetterImageButton(Asset<Texture2D> buttonTexture) {
        _buttonTexture = buttonTexture;
        _text = null;
        textSize = 1f;
        Width.Set(buttonTexture.Value.Width, 0f);
        Height.Set(buttonTexture.Value.Height, 0f);
    }


    //Remember to use ImmediateLoad request mode if you request the texture in the parameter!
    public UIBetterImageButton(Asset<Texture2D> buttonTexture, string text = null, float textSize = 1f) {
        _buttonTexture = buttonTexture;
        _text = text;
        this.textSize = textSize;
        Width.Set(buttonTexture.Value.Width, 0f);
        Height.Set(buttonTexture.Value.Height, 0f);
    }

    public UIBetterImageButton(Asset<Texture2D> buttonTexture, LocalizedText text = null, float textSize = 1f) {
        _buttonTexture = buttonTexture;
        _text = text;
        this.textSize = textSize;
        Width.Set(buttonTexture.Value.Width, 0f);
        Height.Set(buttonTexture.Value.Height, 0f);
    }

    public override void OnInitialize() {
        if (_text is null) {
            return;
        }

        if (_text is LocalizedText translation) {
            buttonText = new UIBetterText(translation, textSize) {
                HAlign = 0.5f,
                VAlign = 0.5f,
                horizontalTextConstraint = GetDimensions().Width,
                IgnoresMouseInteraction = true
            };
        }
        else {
            buttonText = new UIBetterText(_text as string, textSize) {
                HAlign = 0.5f,
                VAlign = 0.5f,
                horizontalTextConstraint = GetDimensions().Width,
                IgnoresMouseInteraction = true
            };
        }

        Append(buttonText);
    }

    public override void MouseOver(UIMouseEvent evt) {
        if (!isVisible) {
            return;
        }

        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void LeftClick(UIMouseEvent evt) {
        if (!isVisible) {
            return;
        }

        base.LeftClick(evt);
        ProperOnClick?.Invoke(evt, this);
    }

    public void SetHoverImage(Asset<Texture2D> texture) => _borderTexture = texture;

    public void SetImage(Asset<Texture2D> texture) {
        _buttonTexture = texture;
        Width.Set(_buttonTexture.Width(), 0f);
        Height.Set(_buttonTexture.Height(), 0f);

        if (buttonText is not null) {
            buttonText.horizontalTextConstraint = _buttonTexture.Width();
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

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        if (buttonText is not null) {
            buttonText.isVisible = isVisible;
        }

        if (!isVisible) {
            return;
        }
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

    /// <summary>
    /// Simple action/event that triggers after every frame while the mouse is currently
    /// hovering over this element.
    /// </summary>
    public event Action WhileHovering;

    /// <summary>
    /// An "override" of the normal OnClick event that takes into account visibility of this
    /// button. USE THIS INSTEAD OF THE VANILLA EVENT!
    /// </summary>
    public event MouseEvent ProperOnClick;
}