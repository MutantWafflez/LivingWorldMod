using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

/// <summary>
///     A better version of Vanilla's UIItemIcon class. Can use position or the center to draw from,
///     and has hover tooltip functionality.
/// </summary>
public class UIBetterItemIcon (Item displayedItem, float sizeLimit, bool drawFromCenter) : UIElement {
    /// <summary>
    ///     The color to forcefully draw the item as, regardless of anything else. Null means
    ///     no overriding.
    /// </summary>
    public Color? overrideDrawColor = null;

    private Item _displayedItem = displayedItem;

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        //Adapted Vanilla Code
        Main.instance.LoadItem(_displayedItem.type);

        Texture2D itemTexture = TextureAssets.Item[_displayedItem.type].Value;
        Rectangle itemAnimFrame = Main.itemAnimations[_displayedItem.type] == null ? itemTexture.Frame() : Main.itemAnimations[_displayedItem.type].GetFrame(itemTexture);

        Color currentColor = Color.White;
        float itemLightScale = 1f;
        float sizeConstraint = 1f;

        ItemSlot.GetItemLight(ref currentColor, ref itemLightScale, _displayedItem);
        if (overrideDrawColor is { } color) {
            currentColor = color;
        }

        sizeConstraint *= itemLightScale;

        if (itemAnimFrame.Width > sizeLimit || itemAnimFrame.Height > sizeLimit) {
            sizeConstraint = itemAnimFrame.Width <= itemAnimFrame.Height ? sizeLimit / itemAnimFrame.Height : sizeLimit / itemAnimFrame.Width;
        }

        sizeConstraint *= _displayedItem.scale;

        spriteBatch.Draw(
            itemTexture,
            drawFromCenter ? GetDimensions().Center() : GetDimensions().Position(),
            itemAnimFrame,
            currentColor,
            0f,
            drawFromCenter ? new Vector2(itemAnimFrame.Width / 2f, itemAnimFrame.Height / 2f) : default(Vector2),
            sizeConstraint,
            SpriteEffects.None,
            0f
        );

        //Non-vanilla code
        if (ContainsPoint(Main.MouseScreen)) {
            ItemSlot.MouseHover(ref _displayedItem);
        }
    }

    public void SetItem(Item newItem) {
        _displayedItem = newItem;
        Recalculate();
    }

    public void SetItem(int newItemType) {
        _displayedItem?.SetDefaults(newItemType);
        Recalculate();
    }
}