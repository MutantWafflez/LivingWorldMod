using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenConditions;
using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenShapes;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Content.Walls.WorldGen;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI.Chat;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds {
    public class PyramidSubworld : Subworld {
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

        /// <summary>
        /// The "fake" paths that are tertiary from the correct path, that always lead to a dead end.
        /// </summary>
        public List<List<PyramidRoom>> FakePaths {
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
            new PassLegacy("Empty Room Fill", FillEmptyRooms),
            new PassLegacy("Room Layout", GenerateRoomLayouts),
            new PassLegacy("Room Foliage", GenerateRoomFoliage),
            new PassLegacy("Debug Draw Paths", DebugDrawPaths)
        };

        private readonly int _totalVanillaSaveOrLoadSteps = 4;

        private Asset<Texture2D> _pyramidBackground;
        private Asset<Texture2D> _pyramidWorldGenBar;
        private bool _isExiting;
        private string _lastStatusText = "";
        private int _vanillaLoadStepsPassed;

        private readonly int _worldBorderPadding = 150;
        private readonly int _roomSideLength = 100;
        private readonly int _gridSideLength = 10;
        private readonly int _bossRoomPadding = 150;
        private Dictionary<string, List<RoomData>> _allRooms;
        private int _spawnTileX;
        private int _spawnTileY;

        public override void Load() {
            _pyramidBackground = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}Backgrounds/Loading/PyramidBG");
            _pyramidWorldGenBar = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}UI/SubworldGeneration/GenPyramidBar");
        }

        public override void SetStaticDefaults() {
            //Load all rooms
            _allRooms = new Dictionary<string, List<RoomData>>();
            ModContent.GetInstance<LivingWorldMod>()
                      .GetFileNames()
                      .Where(file => file.EndsWith(".pyrroom") && !file.EndsWith("StartRoom.pyrroom"))
                      .Select(IOUtils.GetTagFromFile<RoomData>)
                      .ToList()
                      .ForEach(room => {
                          string key = $"{room.gridWidth}x{room.gridHeight}";

                          if (_allRooms.ContainsKey(key)) {
                              _allRooms[key].Add(room);
                          }
                          else {
                              _allRooms[key] = new List<RoomData>() { room };
                          }
                      });
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

        public override void OnLoad() {
            Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>().currentRoom = CorrectPath.First();
        }

        private void Initialize(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Initializing";
            //Default spawn spot, in case generation goes awry
            _spawnTileX = 225;
            _spawnTileY = 245;

            //Generate grid
            Grid = new PyramidRoomGrid(_gridSideLength, _roomSideLength, _worldBorderPadding, WorldGen.genRand);
            Grid.GenerateGrid();

            //Generate correct path first
            CorrectPath = new List<PyramidRoom>() { Grid.GetRoom(WorldGen.genRand.Next(_gridSideLength), 0) };
            PyramidRoom currentRoom = CorrectPath.First();
            //Starter room will always be a normal room
            currentRoom.roomType = PyramidRoomType.Normal;
            currentRoom.pathSearched = true;

            while (currentRoom is not null) {
                PyramidRoom roomBelow = Grid.GetRoomBelow(currentRoom);
                PyramidRoom roomLeft = Grid.GetRoomToLeft(currentRoom);
                PyramidRoom roomRight = Grid.GetRoomToRight(currentRoom);

                List<Tuple<PyramidRoom, double>> movementChoices = new List<Tuple<PyramidRoom, double>>();
                movementChoices.Add(new Tuple<PyramidRoom, double>(roomBelow, 34)); //Down
                movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomLeft, 33), roomLeft is { pathSearched: false }); //Left
                movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomRight, 33), roomRight is { pathSearched: false }); //Right

                PyramidRoom selectedRoom = new WeightedRandom<PyramidRoom>(WorldGen.genRand, movementChoices.ToArray()).Get();
                if (selectedRoom is not null) {
                    CorrectPath.Add(selectedRoom);
                    selectedRoom.pathSearched = true;

                    //Link doors accordingly
                    PyramidDoorDirection currentDoorDirection = PyramidDoorDirection.Top;
                    if (selectedRoom == roomBelow) {
                        currentDoorDirection = PyramidDoorDirection.Down;
                    }
                    else if (selectedRoom == roomLeft) {
                        currentDoorDirection = PyramidDoorDirection.Left;
                    }
                    else if (selectedRoom == roomRight) {
                        currentDoorDirection = PyramidDoorDirection.Right;
                    }

                    ConnectDoors(currentRoom, selectedRoom, currentDoorDirection, GetOppositeDirection(currentDoorDirection));
                }

                currentRoom = selectedRoom;
            }

            //Move spawn point accordingly
            PyramidRoom starterRoom = CorrectPath.First();
            _spawnTileX = starterRoom.region.Center.X;
            _spawnTileY = starterRoom.region.Center.Y;

            //Generate Fake paths along the correct path
            FakePaths = new List<List<PyramidRoom>>();
            GenerateFakePath(CorrectPath, 5, 50);
            int originalCount = FakePaths.Count;
            for (int i = 0; i < originalCount; i++) {
                GenerateFakePath(FakePaths[i], 8, 30);
            }
        }

        private void FillWorld(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Wall Slabs";
            for (int i = 0; i < Main.maxTilesX; i++) {
                for (int j = 0; j < Main.maxTilesY; j++) {
                    progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                    Tile tile = Framing.GetTileSafely(i, j);
                    tile.WallType = (ushort)ModContent.WallType<PyramidBrickWall>();
                    if (i <= _worldBorderPadding || i > Width - _worldBorderPadding || j <= _worldBorderPadding || j > Height - _bossRoomPadding - _worldBorderPadding) {
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

        private void FillEmptyRooms(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Filling the Empty Space";

            for (int i = 0; i < _gridSideLength; i++) {
                progress.Set(i / (float)_gridSideLength);
                List<PyramidRoom> column = Grid.GetRoomColumn(i);

                foreach (PyramidRoom room in column) {
                    Rectangle roomRegion = room.region;

                    for (int x = roomRegion.X; x <= roomRegion.X + roomRegion.Width; x++) {
                        for (int y = roomRegion.Y; y <= roomRegion.Y + roomRegion.Height; y++) {
                            Tile tile = Framing.GetTileSafely(x, y);
                            if (!room.pathSearched) {
                                tile.HasTile = true;
                                tile.TileType = TileID.SandStoneSlab;
                            }
                        }
                    }
                }
            }
        }

        public void GenerateRoomLayouts(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Shifting the Rooms";

            //Manually generate starter room
            PyramidRoom startRoom = CorrectPath.First();
            RoomData startRoomData = IOUtils.GetTagFromFile<RoomData>(LivingWorldMod.LWMStructurePath + "PyramidRooms/StartRoom.pyrroom");
            Rectangle roomRegion = startRoom.region;

            WorldGenUtils.GenerateStructure(startRoomData.roomLayout, roomRegion.X, roomRegion.Y);
            WorldGen.PlaceObject(roomRegion.X + startRoomData.doorData[PyramidDoorDirection.Top].X, roomRegion.Y + startRoomData.doorData[PyramidDoorDirection.Top].Y, ModContent.TileType<PyramidDoorTile>());

            foreach ((PyramidDoorDirection key, PyramidDoorData value) in startRoom.doorData) {
                Point16 doorPos = new Point16(startRoom.region.X + startRoomData.doorData[key].X, startRoom.region.Y + startRoomData.doorData[key].Y);

                value.doorPos = doorPos;
                WorldGen.PlaceObject(doorPos.X, doorPos.Y, ModContent.TileType<InnerPyramidDoorTile>());
            }

            startRoom.generationStep = PyramidRoomGenerationStep.LayoutGenerated;

            //Generate ALL the rooms
            List<List<PyramidRoom>> allPaths = FakePaths.Prepend(CorrectPath).ToList();
            for (int i = 0; i < allPaths.Count; i++) {
                progress.Set(i / (allPaths.Count - 1f));

                GenerateRoomLayoutOnPath(allPaths[i]);
            }
        }

        public void GenerateRoomFoliage(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Uncovering Lost Remains";

            //Generate torches in all rooms
            List<List<PyramidRoom>> allPaths = FakePaths.Prepend(CorrectPath).ToList();
            for (int i = 0; i < allPaths.Count; i++) {
                progress.Set(i / (allPaths.Count - 1f));

                GenerateRoomFoliageOnPath(allPaths[i]);
            }
        }

        private void DebugDrawPaths(GenerationProgress progress, GameConfiguration config) {
            if (!LivingWorldMod.IsDebug) {
                return;
            }
            progress.Message = "Visualizing Paths";

            for (int i = 0; i < CorrectPath.Count - 1; i++) {
                if (CorrectPath[i + 1] is null) {
                    break;
                }
                Point firstCenter = CorrectPath[i].region.Center;
                Point secondCenter = CorrectPath[i + 1].region.Center;

                WorldUtils.Gen(firstCenter, new StraightLine(2f, secondCenter), Actions.Chain(
                    new Modifiers.SkipTiles((ushort)ModContent.TileType<InnerPyramidDoorTile>(), (ushort)ModContent.TileType<PyramidDoorTile>()),
                    new Modifiers.Offset(0, -1),
                    new Modifiers.SkipTiles((ushort)ModContent.TileType<InnerPyramidDoorTile>(), (ushort)ModContent.TileType<PyramidDoorTile>()),
                    new Modifiers.Offset(0, 1),
                    new Actions.PlaceTile(TileID.LivingCursedFire)));
            }

            foreach (List<PyramidRoom> fakePath in FakePaths) {
                for (int i = 0; i < fakePath.Count - 1; i++) {
                    Point firstCenter = fakePath[i].region.Center;
                    Point secondCenter = fakePath[i + 1].region.Center;

                    WorldUtils.Gen(firstCenter, new StraightLine(2f, secondCenter), Actions.Chain(
                        new Modifiers.SkipTiles((ushort)ModContent.TileType<InnerPyramidDoorTile>(), (ushort)ModContent.TileType<PyramidDoorTile>()),
                        new Modifiers.Offset(0, -1),
                        new Modifiers.SkipTiles((ushort)ModContent.TileType<InnerPyramidDoorTile>(), (ushort)ModContent.TileType<PyramidDoorTile>()),
                        new Modifiers.Offset(0, 1),
                        new Actions.PlaceTile(TileID.LivingFire)));
                }
            }
        }

        /// <summary>
        /// Scans the passed in path and randomly generates more fake paths branching off of it.
        /// Make sure the fake path list is properly initialized before calling this.
        /// </summary>
        /// <param name="pathToSearchAlong"> The path this method will search along to potentially branch off of. </param>
        /// <param name="branchOccurrenceDenominator">
        /// The starting value of 1/value, determining whether or not a new path will be created. Every time the RNG fails,
        /// this value will decrease by 1 until success, when it is reset to this value.
        /// </param>
        /// <param name="branchEndChanceDenominator">
        /// The starting value of 1/value, determining whether or not a path generating will end. Every time the RNG fails,
        /// this value will decrease by 5 until success, when it is reset to this value.
        /// </param>
        private void GenerateFakePath(List<PyramidRoom> pathToSearchAlong, int branchOccurrenceDenominator, int branchEndChanceDenominator) {
            int branchChanceDenominator = branchOccurrenceDenominator;

            foreach (PyramidRoom originalPathRoom in pathToSearchAlong) {
                if (originalPathRoom is null || Grid.GetRoomBelow(originalPathRoom) is { pathSearched: true } && Grid.GetRoomToLeft(originalPathRoom) is { pathSearched: true } && Grid.GetRoomToRight(originalPathRoom) is { pathSearched: true }) {
                    continue;
                }

                if (WorldGen.genRand.NextBool(branchChanceDenominator)) {
                    branchChanceDenominator = branchOccurrenceDenominator;
                    int endChanceDenominator = branchEndChanceDenominator;

                    List<PyramidRoom> newPath = new List<PyramidRoom>() { originalPathRoom };
                    PyramidRoom currentFakeRoom = newPath[0];

                    while (true) {
                        PyramidRoom roomBelow = Grid.GetRoomBelow(currentFakeRoom);
                        PyramidRoom roomLeft = Grid.GetRoomToLeft(currentFakeRoom);
                        PyramidRoom roomRight = Grid.GetRoomToRight(currentFakeRoom);

                        List<Tuple<PyramidRoom, double>> movementChoices = new List<Tuple<PyramidRoom, double>>();
                        movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomBelow, 25), roomBelow is { pathSearched: false }); //Down
                        movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomLeft, 37.5), roomLeft is { pathSearched: false }); //Left
                        movementChoices.AddConditionally(new Tuple<PyramidRoom, double>(roomRight, 37.5), roomRight is { pathSearched: false }); //Right
                        if (!movementChoices.Any()) {
                            break;
                        }

                        PyramidRoom selectedRoom = new WeightedRandom<PyramidRoom>(WorldGen.genRand, movementChoices.ToArray()).Get();
                        if (WorldGen.genRand.NextBool(endChanceDenominator) || selectedRoom is null || selectedRoom.pathSearched) {
                            break;
                        }

                        //Link doors accordingly
                        PyramidDoorDirection currentDoorDirection = PyramidDoorDirection.Top;
                        if (selectedRoom == roomBelow) {
                            if (currentFakeRoom.doorData.ContainsKey(PyramidDoorDirection.Down)) {
                                break;
                            }

                            currentDoorDirection = PyramidDoorDirection.Down;
                        }
                        else if (selectedRoom == roomLeft) {
                            if (currentFakeRoom.doorData.ContainsKey(PyramidDoorDirection.Left)) {
                                break;
                            }

                            currentDoorDirection = PyramidDoorDirection.Left;
                        }
                        else if (selectedRoom == roomRight) {
                            if (currentFakeRoom.doorData.ContainsKey(PyramidDoorDirection.Right)) {
                                break;
                            }

                            currentDoorDirection = PyramidDoorDirection.Right;
                        }
                        ConnectDoors(currentFakeRoom, selectedRoom, currentDoorDirection, GetOppositeDirection(currentDoorDirection));

                        newPath.Add(selectedRoom);
                        selectedRoom.pathSearched = true;

                        endChanceDenominator = (int)MathHelper.Clamp(endChanceDenominator - 5, 1, branchEndChanceDenominator);
                        currentFakeRoom = selectedRoom;
                    }

                    if (newPath.Count > 1) {
                        FakePaths.Add(newPath);
                    }
                }
                else {
                    branchChanceDenominator = (int)MathHelper.Clamp(branchChanceDenominator - 1, 1, branchChanceDenominator);
                }
            }
        }

        /// <summary>
        /// Generates all the room layouts in the passed in path.
        /// </summary>
        private void GenerateRoomLayoutOnPath(List<PyramidRoom> path) {
            for (int i = 0; i < path.Count; i++) {
                PyramidRoom room = path[i];

                Rectangle roomRegion = room.region;
                if (room.generationStep >= PyramidRoomGenerationStep.LayoutGenerated) {
                    continue;
                }
                room.generationStep = PyramidRoomGenerationStep.LayoutGenerated;

                string roomDimensions = $"{room.gridWidth}x{room.gridHeight}";
                RoomData roomData = WorldGen.genRand.Next(_allRooms[roomDimensions]);

                //Generate Layout
                WorldGenUtils.GenerateStructure(roomData.roomLayout, roomRegion.X, roomRegion.Y, false);

                //Initialize doors and place them
                foreach ((PyramidDoorDirection key, PyramidDoorData value) in room.doorData) {
                    Point16 doorPos = new Point16(room.region.X + roomData.doorData[key].X, room.region.Y + roomData.doorData[key].Y);

                    value.doorPos = doorPos;
                    WorldGen.PlaceObject(doorPos.X, doorPos.Y, ModContent.TileType<InnerPyramidDoorTile>());
                }
            }
        }

        /// <summary>
        /// Generates "foliage" in the rooms on the passed in path.
        /// </summary>
        private void GenerateRoomFoliageOnPath(List<PyramidRoom> path) {
            for (int i = 0; i < path.Count; i++) {
                PyramidRoom room = path[i];

                Rectangle roomRegion = room.region;
                if (room.generationStep >= PyramidRoomGenerationStep.FoliageGenerated) {
                    continue;
                }
                room.generationStep = PyramidRoomGenerationStep.FoliageGenerated;

                TileObjectData pileData = TileObjectData.GetTileData(TileID.LargePiles, 0);
                List<Point> pileLocations = new List<Point>();

                //Search through entire room, looking for valid pile placements
                for (int x = roomRegion.X; x < roomRegion.X + roomRegion.Width; x++) {
                    for (int y = roomRegion.Y; y <= roomRegion.Y + roomRegion.Height; y++) {
                        Point searchPoint = new Point(x, y);

                        if (TileObject.CanPlace(x, y, TileID.LargePiles, 0, -1, out _) && WorldUtils.Find(searchPoint, Searches.Chain(new Searches.Down(1), new IsDry().AreaAnd(pileData.Width, pileData.Height)), out _)) {
                            pileLocations.Add(searchPoint);
                        }
                    }
                }

                foreach (Point pileLocation in pileLocations) {
                    //34% chance to place by default
                    if (!WorldGen.genRand.NextBool(5)) {
                        continue;
                    }
                    Dictionary<ushort, int> tileData = new Dictionary<ushort, int>();

                    //Check if there is already any piles nearby; if not, place!
                    WorldUtils.Gen(pileLocation + new Point(1, 0), new Shapes.Circle(28), new Actions.TileScanner(TileID.LargePiles).Output(tileData));
                    if (tileData[TileID.LargePiles] == 0) {
                        WorldGen.PlaceObject(pileLocation.X, pileLocation.Y, TileID.LargePiles, style: WorldGen.genRand.Next(6));
                    }
                }
            }
        }

        /// <summary>
        /// Connects the passed in rooms' two doors, whose direction is also specified.
        /// </summary>
        private void ConnectDoors(PyramidRoom firstRoom, PyramidRoom secondRoom, PyramidDoorDirection firstDoorDirection, PyramidDoorDirection secondDoorDirection) {
            firstRoom.doorData[firstDoorDirection] = new PyramidDoorData();
            secondRoom.doorData[secondDoorDirection] = new PyramidDoorData();

            firstRoom.doorData[firstDoorDirection].linkedDoor = secondRoom.doorData[secondDoorDirection];
            secondRoom.doorData[secondDoorDirection].linkedDoor = firstRoom.doorData[firstDoorDirection];
        }

        /// <summary>
        /// Returns the opposite direction of the passed in door direction.
        /// </summary>
        private PyramidDoorDirection GetOppositeDirection(PyramidDoorDirection direction) => direction.NextEnum().NextEnum();
    }
}