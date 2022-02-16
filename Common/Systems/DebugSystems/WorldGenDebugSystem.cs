using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenShapes;
using LivingWorldMod.Content.Walls.WorldGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.Systems.DebugSystems {
    /// <summary>
    /// Debug ModSystem used for testing world generation code.
    /// </summary>
    public class WorldGenDebugSystem : ModSystem {
        public override bool IsLoadingEnabled(Mod mod) => LivingWorldMod.IsDebug;

        public override void PostUpdateEverything() {
            //Trigger the generation method by pressing 0 on the numpad
            if (Main.keyState.IsKeyDown(Keys.NumPad0) && !Main.oldKeyState.IsKeyDown(Keys.NumPad0)) {
                GenerationMethod((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
            }
        }

        private void GenerationMethod(int x, int y) {
            Dust.QuickBox(new Vector2(x, y) * 16, new Vector2(x + 2, y + 3) * 16, 2, Color.YellowGreen, null);

            // Code to test placed here:

            Point tipOfPyramid = new Point(x, y);

            ShapeData fullPyramidData = new ShapeData();

            //Generate outer-shell of the pyramid, solid in the beginning
            WorldUtils.Gen(tipOfPyramid, new EqualTriangle(271), Actions.Chain(
                new Actions.SetTile(TileID.SandStoneSlab, true),
                new Actions.PlaceWall((ushort)ModContent.WallType<PyramidBrickWall>()),
                new Actions.ContinueWrapper(Actions.Chain(
                    new Modifiers.IsTouchingAir(),
                    new Actions.RemoveWall()
                )),
                new Actions.Blank().Output(fullPyramidData)
            ));

            ShapeData alteredPyramidData = new ShapeData(fullPyramidData);
        }
    }
}