using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

public class UITooltipElement(LocalizedText tooltipText) : UIElement {
    private static readonly Item DummyItem = new(ItemID.None, 0) {
        // Can be any item - just can't be 0 (otherwise the tooltip won't draw)
        type = ItemID.IronPickaxe
    };

    public LocalizedText tooltipText = tooltipText;

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);

        if (!ContainsPoint(Main.MouseScreen)) {
            return;
        }

        DummyItem.SetNameOverride(tooltipText.Value);
        Main.HoverItem = DummyItem;
        Main.instance.MouseText("");
        Main.mouseText = true;
    }
}