using Microsoft.Xna.Framework;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

public class UISquarePanel : UIElement {
    public UISimpleRectangle borderRectangle;
    public UISimpleRectangle innerRectangle;

    public UISquarePanel() : this(Color.Black, Color.White) { }

    public UISquarePanel(Color borderColor, Color backgroundColor, int borderSize = 2) {
        borderRectangle = new UISimpleRectangle(borderColor) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        borderRectangle.SetPadding(borderSize);

        innerRectangle = new UISimpleRectangle(backgroundColor) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        borderRectangle.Append(innerRectangle);

        Append(borderRectangle);
    }
}