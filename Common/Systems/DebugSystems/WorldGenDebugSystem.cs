#if DEBUG

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems.DebugSystems {

    /// <summary>
    /// Debug ModSystem used for testing world generation code.
    /// </summary>
    public class WorldGenDebugSystem : ModSystem {

        public override void PostUpdateEverything() {
            //Trigger the generation method by pressing 0 on the numpad
            if (Main.keyState.IsKeyDown(Keys.NumPad0) && !Main.oldKeyState.IsKeyDown(Keys.NumPad0)) {
                GenerationMethod((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
            }
        }

        private void GenerationMethod(int x, int y) {
            Dust.QuickBox(new Vector2(x, y) * 16, new Vector2(x + 1, y + 1) * 16, 2, Color.YellowGreen, null);

            // Code to test placed here:
        }
    }
}

#endif