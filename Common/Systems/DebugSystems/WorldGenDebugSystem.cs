#if DEBUG

using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.Systems.DebugSystems {

    /// <summary>
    /// Debug ModSystem used for testing world generation code.
    /// </summary>
    public class WorldGenDebugSystem : ModSystem {

        public static bool JustPressed(Keys key) {
            return Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
        }

        public override void PostUpdateEverything() {
            //Trigger the generation method by pressing 0 on the numpad
            if (JustPressed(Keys.NumPad0)) {
                GenerationMethod((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
            }
        }

        private void GenerationMethod(int x, int y) {
            Dust.QuickBox(new Vector2(x, y) * 16, new Vector2(x + 1, y + 1) * 16, 2, Color.YellowGreen, null);

            // Code to test placed here:
            float xScale = WorldGen.genRand.NextFloat(1.5f, 1.75f);
            float yScale = WorldGen.genRand.NextFloat(0.5f, 0.8f);
            int radius = 35;
            Point originPoint = new Point(x, y);

            ShapeData fullShapeData = new ShapeData();

            //Generate base-cloud structure
            WorldUtils.Gen(originPoint, new Shapes.Slime(radius, xScale, yScale), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Actions.SetTile(TileID.Cloud, true),
                new Modifiers.Blotches(2, 1, 0.9f),
                new Actions.PlaceWall(WallID.Cloud),
                new Actions.SetFrames(),
                new Actions.Blank().Output(fullShapeData)
            ));

            //Generate dirt within cloud base
            WorldUtils.Gen(originPoint, new Shapes.Slime(radius, xScale * 0.75f, yScale * 0.75f), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Modifiers.Offset(0, -(int)(radius * yScale * 0.215f)),
                new Modifiers.Blotches(2, 1, 0.9f),
                new Actions.SetTile(TileID.Dirt),
                new Actions.PlaceWall(WallID.Cloud),
                new Actions.SetFrames(),
                new Actions.Blank().Output(fullShapeData)
            ));

            //Generate small pond/lake
            WorldUtils.Gen(originPoint, new Shapes.Slime(radius, xScale * 0.25f, yScale * 0.25f), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Modifiers.Offset(0, -(int)(radius * yScale * 0.465f)),
                new Actions.ClearTile(true),
                new Actions.ClearWall(true),
                new Actions.SetLiquid(),
                new Actions.Blank().Output(fullShapeData)
            ));

            WorldUtils.Gen(originPoint, new ModShapes.All(fullShapeData), Actions.Chain(
                //Remove walls on outer-most tiles
                new Actions.ContinueWrapper(Actions.Chain(
                    new Modifiers.IsTouchingAir(),
                    new Actions.RemoveWall()
                )),
                //Create Grass on dirt tiles
                new Actions.ContinueWrapper(Actions.Chain(
                    new Modifiers.Conditions(new Conditions.IsTile(TileID.Dirt)),
                    new Modifiers.IsTouchingAir(true),
                    new Actions.Custom((i, j, args) => {
                        WorldGen.SpreadGrass(i, j, repeat: false);
                        return true;
                    })
                )),
                //Smooth Tiles
                new Actions.Smooth()
            ));
        }
    }
}

#endif