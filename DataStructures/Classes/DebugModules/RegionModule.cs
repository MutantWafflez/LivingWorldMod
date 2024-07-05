using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria.DataStructures;

namespace LivingWorldMod.DataStructures.Classes.DebugModules;

/// <summary>
///     Abstract module used for applying effects to a certain region of tiles.
/// </summary>
public abstract class RegionModule : DebugModule {
    protected Point16 topLeft = Point16.NegativeOne;

    protected Point16 bottomRight = Point16.NegativeOne;

    private bool _isDoingEffect;

    public override void KeysPressed(Keys[] pressedKeys) {
        if (pressedKeys.Contains(Keys.NumPad1)) {
            topLeft = new Point16((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));
            Main.NewText("Top Left Set to: " + topLeft.X + ", " + topLeft.Y);
        }

        if (pressedKeys.Contains(Keys.NumPad2)) {
            bottomRight = new Point16((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));
            Main.NewText("Bottom Right Set to: " + bottomRight.X + ", " + bottomRight.Y);
        }

        if (pressedKeys.Contains(Keys.NumPad3) && !_isDoingEffect && topLeft != Point16.NegativeOne && bottomRight != Point16.NegativeOne) {
            Main.NewText("Applying Effect...");
            _isDoingEffect = true;
            ApplyEffectOnRegion();
            _isDoingEffect = false;
        }
    }

    public override void ModuleUpdate() {
        if (topLeft != Point16.NegativeOne && bottomRight != Point16.NegativeOne) {
            Dust.QuickBox(topLeft.ToWorldCoordinates(Vector2.Zero), bottomRight.ToWorldCoordinates(new Vector2(16f)), 8, Color.YellowGreen, null);
        }
    }

    /// <summary>
    ///     This is where you apply the effects on the square region once the
    ///     NumPad3 key is pressed.
    /// </summary>
    protected abstract void ApplyEffectOnRegion();
}