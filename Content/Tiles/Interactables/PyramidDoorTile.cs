using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.Subworlds;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Interactables {
    /// <summary>
    /// Not an actual door in a traditional sense; it looks like one, but right clicking doesn't
    /// actually change the tile itself. Allows entrance into the Revamped Pyramid Subworld.
    /// </summary>
    public class PyramidDoorTile : BaseTile {
        public static readonly SoundStyle OpeningDoorSound = new SoundStyle($"{LivingWorldMod.LWMSoundPath}Tiles/PyramidDoorOpen");

        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSolidTop[Type] = true;

            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 4, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);

            AddMapEntry(Color.Orange);

            AnimationFrameHeight = 432;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j);
            PyramidDoorSystem doorSystem = ModContent.GetInstance<PyramidDoorSystem>();

            if (doorSystem.DoorBeingOpenedPosition == topLeft) {
                frameYOffset = (int)MathHelper.Clamp(doorSystem.DoorAnimationPhase - 1, 0f, PyramidDoorSystem.LastDoorAnimationPhase) * 72;
            }
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
            effectOnly = !LivingWorldMod.IsDebug;
            fail = !LivingWorldMod.IsDebug;
        }

        public override bool RightClick(int i, int j) {
            if (SubworldSystem.IsActive<PyramidDimension>()) {
                SubworldSystem.Exit();

                return true;
            }
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j);

            Main.LocalPlayer.Teleport(topLeft.ToWorldCoordinates(22, 22), -1);

            ModContent.GetInstance<PyramidDoorSystem>().StartDoorOpen(topLeft);

            SoundEngine.PlaySound(OpeningDoorSound with { Pitch = -1f }, topLeft.ToWorldCoordinates(32));

            return true;
        }
    }
}