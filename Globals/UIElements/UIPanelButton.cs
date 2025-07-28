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
///     UIPanel extension that has functionality for being a button.
/// </summary>
public class UIPanelButton : UIPanel {
    public UIModifiedText buttonText;

    public float textSize;

    /// <summary>
    ///     Whether or not, while the mousing is hovering over this element, the player can use an
    ///     item (mouseInterface = true). Defaults to true.
    /// </summary>
    public bool preventItemUsageWhileHovering = true;

    private object _text;

    private float _activeVisibility = 1f;

    private float _inactiveVisibility = 0.4f;

    public UIPanelButton(Asset<Texture2D> customBackground, Asset<Texture2D> customBorder, int customCornerSize = 12, int customBarSize = 4, LocalizedText text = null, float textSize = 1f)
        : base(customBackground, customBorder, customCornerSize, customBarSize) {
        _text = text;
        this.textSize = textSize;

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
    }

    public override void MouseOver(UIMouseEvent evt) {
        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
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

    /// <summary>
    ///     Simple action/event that triggers after every frame while the mouse is currently
    ///     hovering over this element.
    /// </summary>
    public event Action WhileHovering;
}