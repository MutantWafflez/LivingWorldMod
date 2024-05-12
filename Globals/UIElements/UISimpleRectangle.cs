using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI;

namespace LivingWorldMod.Globals.UIElements;

public class UISimpleRectangle(Color color) : UIElement {
    public Color color = color;

    public UISimpleRectangle() : this(Color.White) { }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle dimensions = GetDimensions();
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            dimensions.ToRectangle(),
            null,
            color,
            0f,
            Vector2.Zero,
            SpriteEffects.None,
            0.0f
        );
    }
}