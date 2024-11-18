using LivingWorldMod.DataStructures.Structs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

public class UITooltipElement(DynamicLocalizedText text) : UIElement {
    private static readonly Item DummyItem = new(ItemID.None, 0) {
        // Can be any item - just can't be 0 (otherwise the tooltip won't draw)
        type = ItemID.IronPickaxe
    };

    private string _formattedTooltipText = text.SubstitutedText;

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);

        if (!ContainsPoint(Main.MouseScreen)) {
            return;
        }

        DummyItem.SetNameOverride(_formattedTooltipText);
        Main.HoverItem = DummyItem;
        Main.instance.MouseText("");
        Main.mouseText = true;
    }

    public void SetText(DynamicLocalizedText newText) {
        text = newText;
        _formattedTooltipText = newText.SubstitutedText;
    }

    public void ReformatText(object formatSubstitutesObject = null) {
        text = new DynamicLocalizedText(text.text, formatSubstitutesObject);
        _formattedTooltipText = text.SubstitutedText;
    }
}