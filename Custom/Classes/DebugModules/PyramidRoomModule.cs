using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.VanillaOverrides.WorldGen.GenConditions;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Custom.Classes.DebugModules {
    /// <summary>
    /// Module that is basically the same as <seealso cref="StructureModule"/>, but has additional functionality
    /// for specifically the Revamped Pyramid rooms.
    /// </summary>
    public class PyramidRoomModule : StructureModule {
        private Point16 _topDoorPos = Point16.Zero;

        private Point16 _rightDoorPos = Point16.Zero;

        private Point16 _leftDoorPos = Point16.Zero;

        private Point16 _downDoorPos = Point16.Zero;

        public override void KeysPressed(Keys[] pressedKeys) {
            base.KeysPressed(pressedKeys);
            Point16 mousePos = new((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));

            if (pressedKeys.Contains(Keys.Up) && DoValidDoorPosCheck(mousePos)) {
                _topDoorPos = mousePos;
                Main.NewText($"Top Door Position: {_topDoorPos}");
            }
            if (pressedKeys.Contains(Keys.Right) && DoValidDoorPosCheck(mousePos)) {
                _rightDoorPos = mousePos;
                Main.NewText($"Right Door Position: {_rightDoorPos}");
            }
            if (pressedKeys.Contains(Keys.Left) && DoValidDoorPosCheck(mousePos)) {
                _leftDoorPos = mousePos;
                Main.NewText($"Left Door Position: {_leftDoorPos}");
            }
            if (pressedKeys.Contains(Keys.Down) && DoValidDoorPosCheck(mousePos)) {
                _downDoorPos = mousePos;
                Main.NewText($"Down Door Position: {_downDoorPos}");
            }
        }

        public override void ModuleUpdate() {
            base.ModuleUpdate();
            int doorType = ModContent.TileType<InnerPyramidDoorTile>();
            TileObjectData doorData = TileObjectData.GetTileData(doorType, 0);

            if (_topDoorPos != Point16.Zero) {
                Dust.QuickBox(_topDoorPos.ToWorldCoordinates(Vector2.Zero), _topDoorPos.ToWorldCoordinates(new Vector2(16f * doorData.Width, 16f * doorData.Height)), 2, Color.OrangeRed, null);
            }
            if (_rightDoorPos != Point16.Zero) {
                Dust.QuickBox(_rightDoorPos.ToWorldCoordinates(Vector2.Zero), _rightDoorPos.ToWorldCoordinates(new Vector2(16f * doorData.Width, 16f * doorData.Height)), 2, Color.OrangeRed, null);
            }
            if (_leftDoorPos != Point16.Zero) {
                Dust.QuickBox(_leftDoorPos.ToWorldCoordinates(Vector2.Zero), _leftDoorPos.ToWorldCoordinates(new Vector2(16f * doorData.Width, 16f * doorData.Height)), 2, Color.OrangeRed, null);
            }
            if (_downDoorPos != Point16.Zero) {
                Dust.QuickBox(_downDoorPos.ToWorldCoordinates(Vector2.Zero), _downDoorPos.ToWorldCoordinates(new Vector2(16f * doorData.Width, 16f * doorData.Height)), 2, Color.OrangeRed, null);
            }
        }

        protected override void ApplyEffectOnRegion() {
            if (topLeft == Point16.Zero || bottomRight == Point16.Zero) {
                Main.NewText("Invalid TopLeft or BottomRight!");
                return;
            }
            if (_topDoorPos == Point16.Zero || _rightDoorPos == Point16.Zero || _leftDoorPos == Point16.Zero || _downDoorPos == Point16.Zero) {
                Main.NewText("Missing Door Position!");
                return;
            }

            List<List<TileData>> tileData = new();

            for (int x = 0; x <= bottomRight.X - topLeft.X; x++) {
                tileData.Add(new List<TileData>());
                for (int y = 0; y <= bottomRight.Y - topLeft.Y; y++) {
                    Tile requestedTile = Framing.GetTileSafely(x + topLeft.X, y + topLeft.Y);
                    tileData[x].Add(new TileData(requestedTile));
                }
            }

            RoomData roomData = new(
                new StructureData(tileData.Count, tileData[0].Count, tileData),
                _topDoorPos - topLeft,
                _rightDoorPos - topLeft,
                _downDoorPos - topLeft,
                _leftDoorPos - topLeft,
                (byte)((bottomRight.X - topLeft.X) / 100),
                (byte)((bottomRight.Y - topLeft.Y) / 100)
            );

            string outputPath = IOUtils.GetLWMFilePath() + $"/PyramidRoom_{DateTime.Now.ToShortTimeString().Replace(':', '_').Replace(' ', '_')}.pyrroom";

            TagIO.ToFile(new TagCompound { { nameof(RoomData), roomData } }, outputPath);

            Main.NewText("Room Copied to File!");
        }

        /// <summary>
        /// Checks and returns whether or not the passed in position is valid for a door to placed.
        /// </summary>
        private bool DoValidDoorPosCheck(Point16 position) {
            int doorType = ModContent.TileType<InnerPyramidDoorTile>();
            TileObjectData doorData = TileObjectData.GetTileData(doorType, 0);

            if (TileObject.CanPlace(position.X, position.Y, doorType, 0, -1, out _) && WorldUtils.Find(position.ToPoint(), Searches.Chain(new Searches.Down(1), new IsDry().AreaAnd(doorData.Width, doorData.Height)), out _)) {
                return true;
            }

            Main.NewText("Invalid Door Position!");
            return false;
        }
    }
}