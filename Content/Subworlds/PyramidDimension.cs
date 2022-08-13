using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenShapes;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Content.Walls.WorldGen;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds {
    public class PyramidDimension : Subworld {
        /// <summary>
        /// The grid that the dungeon is composed of. Each room at maximum can be 100x100.
        /// </summary>
        public PyramidRoomGrid Grid {
            get;
            private set;
        }

        /// <summary>
        /// The "correct" path that leads from the starter room to the boss room.
        /// </summary>
        public List<PyramidRoom> CorrectPath {
            get;
            private set;
        }

        public override int Width => _roomSideLength * _gridSideLength + _worldBorderPadding * 2;

        public override int Height => _roomSideLength * _gridSideLength + _bossRoomPadding + _worldBorderPadding * 2;

        public override bool ShouldSave => false;

        public override List<GenPass> Tasks => new List<GenPass>() {
            new PassLegacy("Initialize", Initialize),
            new PassLegacy("Fill World", FillWorld),
            new PassLegacy("Set Spawn", SetSpawn),
            new PassLegacy("Room Generation", GenerateRooms),
            new PassLegacy("Debug Draw Paths", DebugDrawPaths)
        };

        private readonly int _totalVanillaSaveOrLoadSteps = 4;

        private Asset<Texture2D> _pyramidBackground;
        private Asset<Texture2D> _pyramidWorldGenBar;
        private bool _isExiting;
        private string _lastStatusText = "";
        private int _vanillaLoadStepsPassed;

        private readonly int _worldBorderPadding = 150;
        private readonly int _roomSideLength = 115;
        private readonly int _gridSideLength = 10;
        private readonly int _bossRoomPadding = 150;
        private int _spawnTileX;
        private int _spawnTileY;

        public override void Load() {
            _pyramidBackground = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}Backgrounds/Loading/PyramidBG");
            _pyramidWorldGenBar = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}UI/SubworldGeneration/GenPyramidBar");
        }

        public override void OnEnter() {
            _vanillaLoadStepsPassed = -1;
            _lastStatusText = "";
            _isExiting = false;
        }

        public override void OnExit() {
            _vanillaLoadStepsPassed = -1;
            _lastStatusText = "";
            _isExiting = true;
        }

        public override void DrawMenu(GameTime gameTime) {
            //A bit of a hacky solution, but this avoids reflection; essentially, we want to add the steps of unloading the main world when
            //we are loading into the subworld (or the bar will stay stagnant for awhile which is boring) as a part of the progress, so we
            //check every time the status text changes (which denotes a step was completed)
            string deNumberedStatusText = string.Concat(Main.statusText.Where(character => !char.IsDigit(character)));
            if (deNumberedStatusText != _lastStatusText) {
                _vanillaLoadStepsPassed++;
            }
            _lastStatusText = deNumberedStatusText;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            //Background Draw
            Main.spriteBatch.Draw(_pyramidBackground.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            //Progress Bar Drawing
            Vector2 totalProgBarPos = new Vector2(Main.screenWidth / 2f - 274f, Main.screenHeight / 2f - 18f);
            Vector2 totalProgBarSize = new Vector2(_pyramidWorldGenBar.Width() - 8f, 18f);

            Vector2 passProgBarPos = new Vector2(Main.screenWidth / 2f - 252f, Main.screenHeight / 2f);
            Vector2 passProgBarSize = new Vector2(_pyramidWorldGenBar.Width() - 48f, 12f);

            Color progressBarBackgroundColor = new Color(63, 63, 63);

            //Progress Bar Background Colors
            Main.spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)totalProgBarPos.X, (int)totalProgBarPos.Y, (int)totalProgBarSize.X, (int)totalProgBarSize.Y),
                progressBarBackgroundColor
            );
            Main.spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)passProgBarPos.X, (int)passProgBarPos.Y, (int)passProgBarSize.X, (int)passProgBarSize.Y),
                progressBarBackgroundColor
            );
            //Total Progress Color
            int totalProgBarWidth = (int)(totalProgBarSize.X * (_vanillaLoadStepsPassed / (float)_totalVanillaSaveOrLoadSteps * 0.34f + (WorldGenerator.CurrentGenerationProgress?.TotalProgress ?? (_isExiting ? 1f : 0f) * 0.66f)));
            Main.spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)totalProgBarPos.X, (int)totalProgBarPos.Y, totalProgBarWidth, (int)totalProgBarSize.Y),
                Color.LightCyan
            );
            //Pass Progress Color
            int passProgBarWidth = (int)(passProgBarSize.X * (WorldGenerator.CurrentGenerationProgress?.Value ?? 0f));
            Main.spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)passProgBarPos.X, (int)passProgBarPos.Y, passProgBarWidth, (int)passProgBarSize.Y),
                Color.GhostWhite
            );
            //Frame Sprite
            Main.spriteBatch.Draw(
                _pyramidWorldGenBar.Value,
                Utils.CenteredRectangle(new Vector2(Main.screenWidth, Main.screenHeight) / 2f, _pyramidWorldGenBar.Size()),
                Color.White
            );
            //Text Draw
            string drawnText = (WorldGenerator.CurrentGenerationProgress?.TotalProgress ?? 1) < 1 ? WorldGenerator.CurrentGenerationProgress.Message : Main.statusText;
            ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                FontAssets.DeathText.Value,
                drawnText,
                new Vector2(Main.screenWidth, Main.screenHeight) / 2f - new Vector2(0f, 74f) - FontAssets.DeathText.Value.MeasureString(drawnText) / 2f,
                Color.White,
                0f,
                Vector2.Zero,
                Vector2.One
            );
        }

        private void Initialize(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Initializing";
            _spawnTileX = 225;
            _spawnTileY = 245;

            Grid = new PyramidRoomGrid(_gridSideLength, _roomSideLength, _worldBorderPadding);
            Grid.GenerateGrid();

            CorrectPath = new List<PyramidRoom>() { Grid.GetRoom(WorldGen._genRand.Next(_gridSideLength), 0) };
            PyramidRoom currentRoom = CorrectPath.Last();
            currentRoom.pathSearched = true;

            while (currentRoom is not null) {
                PyramidRoom roomBelow = Grid.GetRoom(currentRoom.gridTopLeftX, currentRoom.gridTopLeftY + currentRoom.gridHeight);
                PyramidRoom roomLeft = Grid.GetRoom(currentRoom.gridTopLeftX - 1, currentRoom.gridTopLeftY);
                PyramidRoom roomRight = Grid.GetRoom(currentRoom.gridTopLeftX + currentRoom.gridWidth, currentRoom.gridTopLeftY);

                List<Tuple<PyramidRoom, double>> movementChoices = new List<Tuple<PyramidRoom, double>>();
                movementChoices.Add(new Tuple<PyramidRoom, double>(roomBelow, 34)); //Down
                movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomLeft, 33), roomLeft is { pathSearched: false }); //Left
                movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomRight, 33), roomRight is { pathSearched: false }); //Right

                PyramidRoom selectedRoom = new WeightedRandom<PyramidRoom>(WorldGen._genRand, movementChoices.ToArray()).Get();
                CorrectPath.Add(selectedRoom);
                if (selectedRoom is not null) {
                    selectedRoom.pathSearched = true;
                }

                currentRoom = selectedRoom;
            }
        }

        private void FillWorld(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Wall Slabs";
            for (int i = 0; i < Main.maxTilesX; i++) {
                for (int j = 0; j < Main.maxTilesY; j++) {
                    progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                    Tile tile = Framing.GetTileSafely(i, j);
                    tile.WallType = (ushort)ModContent.WallType<PyramidBrickWall>();
                    if (i < _worldBorderPadding || i > Width - _worldBorderPadding || j < _worldBorderPadding || j > Height - _bossRoomPadding - _worldBorderPadding) {
                        tile.HasTile = true;
                        tile.TileType = TileID.SandStoneSlab;
                    }
                }
            }
        }

        private void SetSpawn(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Spawn Point";
            Main.spawnTileX = _spawnTileX;
            Main.spawnTileY = _spawnTileY;
        }

        private void GenerateRooms(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Shifting the Rooms";

            for (int i = 0; i < _gridSideLength; i++) {
                progress.Set(i / (float)_gridSideLength);
                List<PyramidRoom> column = Grid.GetRoomColumn(i);

                for (int j = 0; j < column.Count; j++) {
                    Rectangle room = column[j].region;

                    for (int x = room.X; x <= room.X + room.Width; x++) {
                        for (int y = room.Y; y <= room.Y + room.Height; y++) {
                            Tile tile = Framing.GetTileSafely(x, y);
                            if (x == room.X || x == room.X + room.Width || y == room.Y || y == room.Y + room.Height) {
                                tile.HasTile = true;
                                tile.TileType = TileID.SandStoneSlab;
                            }
                        }
                    }
                }
            }
        }

        private void DebugDrawPaths(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Visualizing Paths";

            for (int i = 0; i < CorrectPath.Count; i++) {
                if (CorrectPath[i + 1] is null) {
                    break;
                }
                Point firstCenter = CorrectPath[i].region.Center;
                Point secondCenter = CorrectPath[i + 1].region.Center;

                WorldUtils.Gen(firstCenter, new StraightLine(2f, secondCenter), new Actions.PlaceTile(TileID.Torches));
            }
        }
    }
}