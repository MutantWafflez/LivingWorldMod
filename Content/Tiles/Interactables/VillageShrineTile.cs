﻿using LivingWorldMod.Common.Systems.UI;
using LivingWorldMod.Content.Items.Placeables.Interactables;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Interactables {
    [LegacyName("HarpyShrineTile")]
    public class VillageShrineTile : BaseTile {
        /// <summary>
        /// The displacement for which the tile is placed, used for tile entity shenanigans.
        /// </summary>
        public readonly Point16 tileOrigin = new Point16(1, 2);

        public override Color? TileColorOnMap => Color.Yellow;

        /// <summary>
        /// The tile width of Village Shrines. Used for tile entity placement/destroying calculations.
        /// </summary>
        private readonly int _fullTileWidth = 4;

        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileLavaDeath[Type] = false;

            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.Origin = tileOrigin;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16 };
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.Width = _fullTileWidth;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, _fullTileWidth, 0);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<VillageShrineEntity>().Hook_AfterPlacement, -1, 0, true);

            TileObjectData.addTile(Type);

            AnimationFrameHeight = 90;
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
            //TODO: Re-add tile animation once reputation system re-implemented
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j, _fullTileWidth);
            Tile topLeftTile = Framing.GetTileSafely(topLeft);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            VillageShrineUISystem shrineUISystem = ModContent.GetInstance<VillageShrineUISystem>();
            if (shrineUISystem.correspondingUIState.EntityPosition == new Point16(i, j)) {
                shrineUISystem.CloseShrineState();
            }

            ModContent.GetInstance<VillageShrineEntity>().Kill(i, j);
            //TODO: Add method to drop other shrines when other villages are added
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, 4 * 16, 5 * 16, ModContent.ItemType<HarpyShrineItem>());
        }

        public override bool RightClick(int i, int j) {
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j, _fullTileWidth);

            if (!TileEntityUtils.TryFindModEntity(topLeft.X, topLeft.Y, out VillageShrineEntity entity)) {
                return false;
            }

            entity.RightClicked();

            return true;
        }
    }
}