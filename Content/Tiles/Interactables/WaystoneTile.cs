﻿using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Core.PacketHandlers;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;

namespace LivingWorldMod.Content.Tiles.Interactables {
    /// <summary>
    /// Class for Waystone tiles, which are basically Pylons but in the wild.
    /// </summary>
    public class WaystoneTile : BasePylon {
        public Asset<Texture2D> waystoneMapIcons;

        public override Color? TileColorOnMap => Color.White;

        /// <summary>
        /// The tile width of Waystones. Used for tile entity placement/destroying calculations.
        /// </summary>
        private readonly int _fullTileWidth = 2;

        public override void Load() {
            waystoneMapIcons = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}MapIcons/WaystoneIcons");
        }

        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileLavaDeath[Type] = false;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
            TileID.Sets.HasOutlines[Type] = true;
            TileID.Sets.FriendlyFairyCanLureTo[Type] = true;

            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.DrawYOffset = 2;

            WaystoneEntity waystoneEntity = ModContent.GetInstance<WaystoneEntity>();
            TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(waystoneEntity.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(waystoneEntity.Hook_AfterPlacement, -1, 0, true);

            TileObjectData.addTile(Type);

            TileID.Sets.InteractibleByNPCs[Type] = true;
            TileID.Sets.PreventsSandfall[Type] = true;

            AddToArray(ref TileID.Sets.CountsAsPylon);

            AnimationFrameHeight = 54;
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => LivingWorldMod.IsDebug;

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j, _fullTileWidth);

            if (TileEntityUtils.TryFindModEntity(topLeft.X, topLeft.Y, out WaystoneEntity foundEntity) && foundEntity.isActivated) {
                frameYOffset += AnimationFrameHeight;
            }
        }

        //Since these "pylons" aren't a traditional vanilla pylon (with no visual crystal), we override the base implementation to prevent it.
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) { }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            // Lightly glow while activated
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j, _fullTileWidth);

            if (TileEntityUtils.TryFindModEntity(topLeft.X, topLeft.Y, out WaystoneEntity foundEntity) && foundEntity.isActivated) {
                Color waystoneColor = foundEntity.WaystoneColor;

                r = waystoneColor.R / 255f;
                g = waystoneColor.G / 255f;
                b = waystoneColor.B / 255f;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            ModContent.GetInstance<WaystoneEntity>().Kill(i, j);
        }

        public override bool RightClick(int i, int j) {
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j, _fullTileWidth);

            if (!TileEntityUtils.TryFindModEntity(topLeft.X, topLeft.Y, out WaystoneEntity entity) || (entity?.isActivated ?? true)) {
                return false;
            }

            WaystoneSystem.Instance.AddNewActivationEntity(topLeft.ToWorldCoordinates(16, 16), entity.WaystoneColor);
            switch (Main.netMode) {
                case NetmodeID.MultiplayerClient:
                    ModPacket packet = ModContent.GetInstance<WaystonePacketHandler>().GetPacket(WaystonePacketHandler.InitiateWaystoneActivation);

                    packet.Write((int)entity.Position.X);
                    packet.Write((int)entity.Position.Y);
                    packet.Send();
                    break;
                case NetmodeID.SinglePlayer:
                    entity.ActivateWaystoneEntity();
                    break;
            }

            return true;
        }

        public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) => true;

        public override bool CanPlacePylon() => true;

        public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
            if (!TileEntityUtils.TryFindModEntity(pylonInfo.PositionInTiles.X, pylonInfo.PositionInTiles.Y, out WaystoneEntity foundEntity) || !foundEntity.isActivated) {
                return;
            }

            bool mouseOver = context.Draw(
                                        waystoneMapIcons.Value,
                                        pylonInfo.PositionInTiles.ToVector2() + new Vector2(1f, 1.5f),
                                        drawColor,
                                        new SpriteFrame(1, 5, 0, (byte)foundEntity.waystoneType),
                                        deselectedScale,
                                        selectedScale,
                                        Alignment.Center)
                                    .IsMouseOver;
            DefaultMapClickHandle(mouseOver, pylonInfo, $"Mods.LivingWorldMod.MapInfo.Waystones.{foundEntity.waystoneType}", ref mouseOverText);
        }
    }
}