using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenConditions;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Enums;
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

            Point origin = new Point(x, y);

            // Test for 2x3 pocket of air
            if (!WorldUtils.Find(origin, Searches.Chain(
                    new Searches.Rectangle(2, 3),
                    new IsAirOrCuttable().AreaAnd(2, 3)
                ), out Point _)) {
                return;
            }

            //Make sure there are two solid tiles below that pocket of air
            if (!WorldUtils.Find(origin + new Point(0, 3), Searches.Chain(
                    new Searches.Rectangle(2, 1),
                    new Conditions.IsSolid().AreaAnd(2, 1)
                ), out _)) {
                return;
            }

            WorldUtils.Gen(origin, new Shapes.Rectangle(2, 3), new Actions.ClearTile(true));
            WorldGen.PlaceObject(x, y, ModContent.TileType<WaystoneTile>(), style: (int)WaystoneType.Desert);

            //WorldUtils.Gen(origin, new EqualTriangle(271), new Actions.SetTile(TileID.SandStoneSlab, true));
        }
    }
}