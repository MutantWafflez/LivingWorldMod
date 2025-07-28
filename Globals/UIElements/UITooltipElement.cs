using LivingWorldMod.DataStructures.Structs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

public class UITooltipElement(DynamicLocalizedText text) : UIElement {
    private static readonly Item DummyItem = new(ItemID.None, 0) {
        // Can be any item - just can't be 0 (otherwise the tooltip won't draw)
        type = ItemID.IronPickaxe
    };

    private string _formattedTooltipText = text.SubstitutedText;
    private DynamicLocalizedText _text = text;

    public UITooltipElement (string text) : this(new LocalizedText(null, text)) { }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);

        if (!IsMouseHovering) {
            return;
        }

        Main.LocalPlayer.mouseInterface = true;

        DummyItem.SetNameOverride(_formattedTooltipText);
        Main.HoverItem = DummyItem;
        Main.instance.MouseText("");
        Main.mouseText = true;
    }

    public void SetText(DynamicLocalizedText newText) {
        _text = newText;
        _formattedTooltipText = newText.SubstitutedText;
    }

    public void ReformatText(object formatSubstitutesObject = null) {
        _text = new DynamicLocalizedText(_text.text, formatSubstitutesObject);
        _formattedTooltipText = _text.SubstitutedText;
    }
}