using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;

namespace LivingWorldMod.Globals.UIElements;

/// <summary>
///     "Better" version of the vanilla scrollbar. Right now, all it does is allow for visibility
///     toggling and prevents item usage.
/// </summary>
public class UIBetterScrollbar : UIScrollbar {
    /// <summary>
    ///     Whether or not, while the mousing is hovering over this element, the player can use an
    ///     item (mouseInterface = true). Defaults to true.
    /// </summary>
    public bool preventItemUsageWhileHovering = true;

    public override void Update(GameTime gameTime) {
        if (ContainsPoint(Main.MouseScreen) && preventItemUsageWhileHovering) {
            Main.LocalPlayer.mouseInterface = true;
        }

        base.Update(gameTime);
    }
}