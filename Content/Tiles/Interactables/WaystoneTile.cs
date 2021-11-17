using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Interactables {
    /// <summary>
    /// Class for Waystone tiles, which are basically Pylons but in the wild.
    /// </summary>
    public class WaystoneTile : BaseTile {
        public WaystoneEntity TileEntity => TileEntitySystem.GetBaseEntityInstance<WaystoneEntity>();

        /// <summary>
        /// The tile width of Waystones. Used for tile entity placement/destroying calculations.
        /// </summary>
        private readonly int _fullTileWidth = 2;

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

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.DrawYOffset = 2;

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(WaystoneEntity.WaystonePlaced, -1, 0, true);

            TileObjectData.addTile(Type);

            AnimationFrameHeight = 54;

            //TODO: Proper localization
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Waystone");
            AddMapEntry(Color.White, name);
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => LivingWorldMod.IsDebug;

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j, _fullTileWidth);

            if (TileEntityUtils.TryFindModEntity(topLeft.X, topLeft.Y, out WaystoneEntity foundEntity) && foundEntity.isActivated) {
                frameYOffset += AnimationFrameHeight;
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            // Lightly glow while activated
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j, _fullTileWidth);

            if (TileEntityUtils.TryFindModEntity(topLeft.X, topLeft.Y, out WaystoneEntity foundEntity) && foundEntity.isActivated) {
                r = 0.5f;
                g = 0.5f;
                b = 0;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => TileEntity.Kill(i, j);

        public override bool RightClick(int i, int j) {
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j, _fullTileWidth);

            if (TileEntityUtils.TryFindModEntity(topLeft.X, topLeft.Y, out WaystoneEntity foundEntity)) {
                foundEntity.RightClicked();
                return true;
            }

            return false;
        }
    }
}