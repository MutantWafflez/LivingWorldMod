using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

/// <summary>
/// UIPanel extension that has functionality for being a button.
/// </summary>
public class UIPanelButton : UIPanel {
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

    private float _activeVisibility = 1f;

    private float _inactiveVisibility = 0.4f;


    public UIPanelButton(Asset<Texture2D> customBackground, Asset<Texture2D> customBorder, int customCornerSize = 12, int customBarSize = 4, string text = null, float textSize = 1f)
        : base(customBackground, customBorder, customCornerSize, customBarSize) {
        _text = text;
        this.textSize = textSize;
    }

    public UIPanelButton(string text = null, float textSize = 1f) {
        _text = text;
        this.textSize = textSize;
    }

    public UIPanelButton(Asset<Texture2D> customBackground, Asset<Texture2D> customBorder, int customCornerSize = 12, int customBarSize = 4, LocalizedText text = null, float textSize = 1f)
        : base(customBackground, customBorder, customCornerSize, customBarSize) {
        _text = text;
        this.textSize = textSize;
    }

    public UIPanelButton(LocalizedText text = null, float textSize = 1f) {
        _text = text;
        this.textSize = textSize;
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

        //Have to do some cheaty shenanigans in order to make the visibility work as normal
        Color oldBackgroundColor = BackgroundColor;
        Color oldBorderColor = BorderColor;
        BackgroundColor = isHovering ? BackgroundColor * _activeVisibility : BackgroundColor * _inactiveVisibility;
        BorderColor = isHovering ? BorderColor * _activeVisibility : BorderColor * _inactiveVisibility;

        base.DrawSelf(spriteBatch);

        BackgroundColor = oldBackgroundColor;
        BorderColor = oldBorderColor;

        //Hovering functionality
        if (!isHovering) {
            return;
        }

        WhileHovering?.Invoke();
        if (preventItemUsageWhileHovering) {
            Main.LocalPlayer.mouseInterface = true;
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