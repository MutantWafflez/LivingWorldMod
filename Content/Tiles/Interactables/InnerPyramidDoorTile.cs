using System;
using System.Linq;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Cutscenes;
using LivingWorldMod.Content.Subworlds.Pyramid;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace LivingWorldMod.Content.Tiles.Interactables {
    /// <summary>
    /// Door that functions as the travel method between rooms WITHIN the Revamped Pyramid.
    /// </summary>
    public class InnerPyramidDoorTile : PyramidDoorTile {
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
            if (playerInCutscene is null) {
                return;
            }
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j);
            CutscenePlayer cutscenePlayer = playerInCutscene.GetModPlayer<CutscenePlayer>();

            if (cutscenePlayer.CurrentCutscene is InnerPyramidDoorCutscene cutscene && cutscene.DoorBeingOpenedPosition == topLeft) {
                frameYOffset = (int)MathHelper.Clamp(cutscene.DoorAnimationPhase - 1, 0f, cutscene.LastDoorAnimationPhase) * 72;
            }
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
            if (Main.netMode == NetmodeID.Server || !SubworldSystem.IsActive<PyramidSubworld>()) {
                return;
            }

            if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0) {
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
            }
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
            //Double check that the tile exists
            Point16 point = new(i, j);
            Tile tile = Main.tile[point.X, point.Y];
            if (tile == null || !tile.HasTile) {
                return;
            }

            //Gets offscreen vector for different lighting modes
            Vector2 offscreenVector = new(Main.offScreenRange);
            if (Main.drawToScreen) {
                offscreenVector = Vector2.Zero;
            }

            PyramidDungeonPlayer dungeonPlayer = Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>();
            PyramidRoom currentRoom = dungeonPlayer.currentRoom;
            //Vanilla's arrow sprite is down by default, so we need to calculate the rotation accordingly
            float rotation = 0f;
            for (PyramidDoorDirection direction = PyramidDoorDirection.Top; (int)direction < Enum.GetValues<PyramidDoorDirection>().Length; direction++) {
                if (currentRoom is null || !currentRoom.doorData.ContainsKey(direction) || currentRoom.doorData[direction].doorPos != point) {
                    continue;
                }

                rotation = MathHelper.ToRadians(180f + 90f * (int)direction);
                goto DoorFound;
            }
            return;

            //Label shenanigans, so that if the door isn't found (and in turn, the rotation), we don't draw the arrow at all
            DoorFound:

            //Arrow framing and such
            Asset<Texture2D> arrowAsset = TextureAssets.GolfBallArrow;
            Rectangle arrowFrame = arrowAsset.Frame(2);
            Rectangle arrowOutlineFrame = arrowAsset.Frame(2, 1, 1);

            //Position calculations
            Vector2 origin = arrowFrame.Size() / 2f;
            float sinusoidalOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * ((float)Math.PI * 2f) / 5f);
            Vector2 drawPos = new Vector2(i, j).ToWorldCoordinates(32f, 36f) + offscreenVector + new Vector2(0f, sinusoidalOffset * 4f);

            //Draw arrow
            Color lightingColor = Lighting.GetColor(point.X, point.Y);
            lightingColor = Color.Lerp(lightingColor, Color.White, 0.8f);
            spriteBatch.Draw(arrowAsset.Value, drawPos - Main.screenPosition, arrowFrame, lightingColor * 0.7f, rotation, origin, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(arrowAsset.Value, drawPos - Main.screenPosition, arrowOutlineFrame, lightingColor * 0.7f, rotation, origin, 1f, SpriteEffects.None, 0f);

            //Add shadowing effect
            float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * ((float)Math.PI * 2f) / 1f) * 0.2f + 0.8f;
            Color shadowColor = new Color(255, 255, 255, 0) * 0.1f * scale;
            for (float shadowPos = 0f; shadowPos < 1f; shadowPos += 1f / 6f) {
                spriteBatch.Draw(arrowAsset.Value, drawPos - Main.screenPosition + ((float)Math.PI * 2f * shadowPos).ToRotationVector2() * (6f + sinusoidalOffset * 2f), arrowFrame, shadowColor, rotation, origin, 1f, SpriteEffects.None, 0f);
            }
        }

        public override bool RightClick(int i, int j) {
            Player player = Main.LocalPlayer;
            CutscenePlayer cutscenePlayer = player.GetModPlayer<CutscenePlayer>();
            if (cutscenePlayer.InCutscene) {
                return true;
            }

            PyramidDungeonPlayer dungeonPlayer = Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>();
            PyramidRoom currentRoom = dungeonPlayer.currentRoom;
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j);
            Vector2 teleportPos = currentRoom.doorData.Values.First(doorData => doorData.doorPos == topLeft).linkedDoor.doorPos.ToWorldCoordinates(16f, 16f);

            InnerPyramidDoorCutscene pyramidCutscene = new(topLeft, teleportPos);
            cutscenePlayer.StartCutscene(pyramidCutscene);
            pyramidCutscene.SendCutscenePacket(-1);

            return true;
        }
    }
}