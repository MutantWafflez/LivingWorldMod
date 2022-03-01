using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenShapes;
using LivingWorldMod.Content.Tiles.Generation;
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
            int lengthOfPyramid = 271;

            ShapeData fullPyramidData = new ShapeData();

            //Generate outer-shell of the pyramid, solid in the beginning
            WorldUtils.Gen(tipOfPyramid, new EqualTriangle(lengthOfPyramid), Actions.Chain(
                new Actions.SetTile(TileID.SandStoneSlab, true),
                new Actions.PlaceWall((ushort)ModContent.WallType<PyramidBrickWall>()),
                new Actions.Blank().Output(fullPyramidData)
            ));

            //Remove walls on outer-most layer
            WorldUtils.Gen(tipOfPyramid, new ModShapes.All(fullPyramidData), Actions.Chain(
                new Modifiers.IsTouchingAir(),
                new Actions.RemoveWall()
            ));

            ShapeData alteredPyramidData = new ShapeData(fullPyramidData);

            //Generate entrance hole
            WorldUtils.Gen(tipOfPyramid + new Point(-15, (int)(lengthOfPyramid * 0.025f)), new Shapes.Rectangle(20, 9), Actions.Chain(
                new Actions.ClearTile(true),
                new Actions.Blank().Output(alteredPyramidData)
            ));

            //Generated cracked bricks below entrance
            WorldUtils.Gen(tipOfPyramid + new Point(-5, 15), new Shapes.Rectangle(10, 5), Actions.Chain(
                new Actions.SetTileKeepWall((ushort)ModContent.TileType<CrackedSandstoneSlab>(), true),
                new Actions.Blank().Output(alteredPyramidData)
            ));

            int firstRoomRadius = 15;
            Point firstRoomOrigin = tipOfPyramid + new Point(0, 20 + firstRoomRadius);

            //Generate static, initial puzzle room
            WorldUtils.Gen(firstRoomOrigin, new Shapes.Slime(firstRoomRadius), Actions.Chain(
                new Actions.ClearTile(true),
                new Actions.Blank().Output(alteredPyramidData)
            ));
        }
    }
}