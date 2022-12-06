using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Subworlds.Pyramid.PyramidGenTasks;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds.Pyramid {
    public sealed class PyramidSubworld : Subworld {
        /// <summary>
        /// Mini-nested class that splits up the generation steps into different classes in
        /// order to de-god class <seealso cref="Pyramid.PyramidSubworld"/>.
        /// </summary>
        public abstract class PyramidGenerationTask {
            public static PyramidSubworld PyramidSubworld => ModContent.GetInstance<PyramidSubworld>();

            /// <summary>
            /// The name of this step.
            /// </summary>
            public abstract string StepName {
                get;
            }

            public abstract void DoTask(GenerationProgress progress, GameConfiguration config);
        }

        /// <summary>
        /// Small enum that denotes the smaller steps of Pyramid Room's generation steps.
        /// </summary>
        public enum PyramidRoomGenerationStep : byte {
            NotGenerated,
            LayoutGenerated,
            CurseGenerated,
            FoliageGenerated
        }

        public const int WorldBorderPadding = 150;
        public const int RoomSideLength = 100;
        public const int GridSideLength = 10;
        public const int BossRoomPadding = 150;

        private const int TotalVanillaSaveOrLoadSteps = 4;

        public override int Width => RoomSideLength * GridSideLength + WorldBorderPadding * 2;

        public override int Height => RoomSideLength * GridSideLength + BossRoomPadding + WorldBorderPadding * 2;

        public override bool ShouldSave => false;

        public override List<GenPass> Tasks => new List<GenPass>().Concat(_genTasks.Select(task => new PassLegacy(task.StepName, task.DoTask))).ToList();

        /// <summary>
        /// The grid that the dungeon is composed of. Each room at maximum can be 100x100.
        /// </summary>
        public PyramidRoomGrid grid;

        /// <summary>
        /// The "correct" path that leads from the starter room to the boss room.
        /// </summary>
        public List<PyramidRoom> correctPath;

        /// <summary>
        /// The "fake" paths that are tertiary from the correct path, that always lead to a dead end.
        /// </summary>
        public List<List<PyramidRoom>> fakePaths;

        public int spawnTileX;
        public int spawnTileY;
        public Dictionary<string, List<RoomData>> allRooms;

        private Asset<Texture2D> _pyramidBackground;
        private Asset<Texture2D> _pyramidWorldGenBar;
        private bool _isExiting;
        private string _lastStatusText = "";
        private int _vanillaLoadStepsPassed;

        private List<PyramidGenerationTask> _genTasks;

        public override void Load() {
            _pyramidBackground = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}Backgrounds/Loading/PyramidBG");
            _pyramidWorldGenBar = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}UI/SubworldGeneration/GenPyramidBar");
        }

        public override void SetStaticDefaults() {
            //Load all rooms
            allRooms = new Dictionary<string, List<RoomData>>();
            ModContent.GetInstance<LivingWorldMod>()
                      .GetFileNames()
                      .Where(file => file.EndsWith(".pyrroom") && !file.EndsWith("StartRoom.pyrroom"))
                      .Select(IOUtils.GetTagFromFile<RoomData>)
                      .ToList()
                      .ForEach(room => {
                          string key = $"{room.gridWidth}x{room.gridHeight}";

                          if (allRooms.ContainsKey(key)) {
                              allRooms[key].Add(room);
                          }
                          else {
                              allRooms[key] = new List<RoomData> { room };
                          }
                      });
            //Load all gen tasks
            _genTasks = new List<PyramidGenerationTask> {
                new InitializePyramidTask(),
                new FillWorldPyramidTask(),
                new SetSpawnPyramidTask(),
                new FillEmptyRoomsPyramidTask(),
                new RoomLayoutsPyramidTask(),
                new CurseGenerationPyramidTask(),
                new RoomFoliagePyramidTask(),
                new DebugPathsPyramidTask()
            };
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
            Vector2 totalProgBarPos = new(Main.screenWidth / 2f - 274f, Main.screenHeight / 2f - 18f);
            Vector2 totalProgBarSize = new(_pyramidWorldGenBar.Width() - 8f, 18f);

            Vector2 passProgBarPos = new(Main.screenWidth / 2f - 252f, Main.screenHeight / 2f);
            Vector2 passProgBarSize = new(_pyramidWorldGenBar.Width() - 48f, 12f);

            Color progressBarBackgroundColor = new(63, 63, 63);

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
            int totalProgBarWidth = (int)(totalProgBarSize.X * (_vanillaLoadStepsPassed / (float)TotalVanillaSaveOrLoadSteps * 0.34f + (WorldGenerator.CurrentGenerationProgress?.TotalProgress ?? (_isExiting ? 1f : 0f) * 0.66f)));
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
            Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>().currentRoom = correctPath.First();
        }

        /// <summary>
        /// Connects the passed in rooms' two doors, whose direction is also specified.
        /// </summary>
        public static void ConnectDoors(PyramidRoom firstRoom, PyramidRoom secondRoom, PyramidDoorDirection firstDoorDirection, PyramidDoorDirection secondDoorDirection) {
            firstRoom.doorData[firstDoorDirection] = new PyramidDoorData();
            secondRoom.doorData[secondDoorDirection] = new PyramidDoorData();

            firstRoom.doorData[firstDoorDirection].linkedDoor = secondRoom.doorData[secondDoorDirection];
            secondRoom.doorData[secondDoorDirection].linkedDoor = firstRoom.doorData[firstDoorDirection];
        }

        /// <summary>
        /// Returns the opposite direction of the passed in door direction.
        /// </summary>
        public static PyramidDoorDirection GetOppositeDirection(PyramidDoorDirection direction) => direction.NextEnum().NextEnum();
    }
}