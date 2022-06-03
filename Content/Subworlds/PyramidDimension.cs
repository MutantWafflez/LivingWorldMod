using System.Collections.Generic;
using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenShapes;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Content.Walls.WorldGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds {
    public class PyramidDimension : Subworld {
        public override int Width => 1000;

        public override int Height => 1000;

        public override bool ShouldSave => !LivingWorldMod.IsDebug;

        public override List<GenPass> Tasks => new List<GenPass>() {
            new PassLegacy("Fill World", FillWorld),
            new PassLegacy("Set Spawn", SetSpawn),
            new PassLegacy("Starter Room", GenStarterRoom),
            new PassLegacy("Initial Tunnel", GenInitialTunnels)
        };

        private Asset<Texture2D> _pyramidBackground;
        private Asset<Texture2D> _pyramidWorldGenBar;

        public override void Load() {
            _pyramidBackground = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}Backgrounds/Loading/PyramidBG");
            _pyramidWorldGenBar = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}UI/SubworldGeneration/GenPyramidBar");
        }

        public override void DrawMenu(GameTime gameTime) {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            //Background Draw
            Main.spriteBatch.Draw(_pyramidBackground.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            //Progress Bar Draw
            Vector2 topProgressBarPos = new Vector2(Main.screenWidth / 2f - 274f, Main.screenHeight / 2f - 18f);
            Vector2 topProgressBarSize = new Vector2(_pyramidWorldGenBar.Width() - 8f, 18f);

            //Background Color
            Main.spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)topProgressBarPos.X, (int)topProgressBarPos.Y, (int)topProgressBarSize.X, (int)topProgressBarSize.Y),
                new Color(63, 63, 63)
            );
            //Actual Progress Color
            Main.spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)topProgressBarPos.X, (int)topProgressBarPos.Y, (int)(topProgressBarPos.X * WorldGenerator.CurrentGenerationProgress?.TotalProgress ?? 1f), (int)topProgressBarSize.Y),
                Color.LightBlue
            );
            //Frame Sprite
            Main.spriteBatch.Draw(
                _pyramidWorldGenBar.Value,
                Utils.CenteredRectangle(new Vector2(Main.screenWidth, Main.screenHeight) / 2f, _pyramidWorldGenBar.Size()),
                Color.White
            );
            //Text Draw
            ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                FontAssets.DeathText.Value,
                (WorldGenerator.CurrentGenerationProgress?.TotalProgress ?? 1) < 1 ? WorldGenerator.CurrentGenerationProgress.Message : Main.statusText,
                new Vector2(Main.screenWidth, Main.screenHeight) / 2f - new Vector2(0f, 74f) - FontAssets.DeathText.Value.MeasureString(Main.statusText) / 2f,
                Color.White,
                0f,
                Vector2.Zero,
                Vector2.One
            );
        }

        private void FillWorld(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Slabbing up";
            for (int i = 0; i < Main.maxTilesX; i++) {
                for (int j = 0; j < Main.maxTilesY; j++) {
                    progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                    Tile tileInQuestion = Framing.GetTileSafely(i, j);
                    tileInQuestion.HasTile = true;
                    tileInQuestion.TileType = TileID.SandStoneSlab;
                    tileInQuestion.WallType = (ushort)ModContent.WallType<PyramidBrickWall>();
                }
            }
        }

        private void SetSpawn(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Spawning up";
            Main.spawnTileX = Width / 2;
            Main.spawnTileY = 82;
        }

        private void GenStarterRoom(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Starter Room";

            //Generate initial triangular room
            WorldUtils.Gen(new Point(Width / 2 - 1, 72), new EqualTriangle(20), new Actions.ClearTile(true));

            //Generate exit door
            WorldGen.PlaceObject(Width / 2 - 2, 78, ModContent.TileType<PyramidDoorTile>(), style: 0, direction: 1);

            //Generate Torches
            WorldGen.PlaceTile(Width / 2 - 3, 77, TileID.Torches, forced: true, style: 16);
            WorldGen.PlaceTile(Width / 2 + 2, 77, TileID.Torches, forced: true, style: 16);
        }

        private void GenInitialTunnels(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Tunnelin'";
            WorldUtils.Gen(new Point(Width / 2 - 7, 81), new StraightLine(5, new Vector2(-10, 10)), new Actions.ClearTile(true));
        }
    }
}