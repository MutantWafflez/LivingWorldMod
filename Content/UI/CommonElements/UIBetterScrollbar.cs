using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace LivingWorldMod.Content.UI.CommonElements {

    /// <summary>
    /// "Better" version of the vanilla scrollbar. Right now, all it does is allow for visibility
    /// toggling and prevents item usage.
    /// </summary>
    public class UIBetterScrollbar : UIScrollbar {

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

        public override void Draw(SpriteBatch spriteBatch) {
            if (!isVisible) {
                return;
            }

            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime) {
            Main.LocalPlayer.mouseInterface = preventItemUsageWhileHovering;

            base.Update(gameTime);
        }
    }
}