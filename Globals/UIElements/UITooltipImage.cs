using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace LivingWorldMod.Globals.UIElements;

public class UITooltipImage : UIImage {
    private static readonly Item DummyItem = new(ItemID.DirtBlock);

    public LocalizedText tooltipText;

    public UITooltipImage(Asset<Texture2D> texture, LocalizedText tooltipText) : base(texture) {
        this.tooltipText = tooltipText;
    }

    public UITooltipImage(Texture2D nonReloadingTexture, LocalizedText tooltipText) : base(nonReloadingTexture) {
        this.tooltipText = tooltipText;
    }

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