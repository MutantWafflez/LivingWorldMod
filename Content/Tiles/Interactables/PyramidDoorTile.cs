using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Cutscenes;
using LivingWorldMod.Content.Subworlds.Pyramid;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Interactables {
    /// <summary>
    /// Not an actual door in a traditional sense; it looks like one, but right clicking doesn't
    /// actually change the tile itself. Allows entrance into the Revamped Pyramid Subworld.
    /// </summary>
    public class PyramidDoorTile : BaseTile {
        /// <summary>
        /// The player, if any, currently within the Enter Pyramid cutscene.
        /// </summary>
        public Player playerInCutscene;

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
            if (playerInCutscene is null) {
                return;
            }
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j);
            CutscenePlayer cutscenePlayer = playerInCutscene.GetModPlayer<CutscenePlayer>();

            if (cutscenePlayer.CurrentCutscene is EnterPyramidCutscene cutscene && cutscene.DoorBeingOpenedPosition == topLeft) {
                frameYOffset = (int)MathHelper.Clamp(cutscene.DoorAnimationPhase - 1, 0f, cutscene.LastDoorAnimationPhase) * 72;
            }
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
            effectOnly = !LivingWorldMod.IsDebug;
            fail = !LivingWorldMod.IsDebug;
        }

        public override bool RightClick(int i, int j) {
            if (SubworldSystem.IsActive<PyramidSubworld>()) {
                SubworldSystem.Exit();

                return true;
            }

            Player player = Main.LocalPlayer;
            CutscenePlayer cutscenePlayer = player.GetModPlayer<CutscenePlayer>();
            if (cutscenePlayer.InCutscene) {
                return true;
            }
            else if (playerInCutscene is not null) {
                SubworldSystem.Enter<PyramidSubworld>();

                return true;
            }
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j);

            EnterPyramidCutscene pyramidCutscene = new EnterPyramidCutscene(topLeft);
            cutscenePlayer.StartCutscene(pyramidCutscene);
            pyramidCutscene.SendCutscenePacket(-1);

            return true;
        }
    }
}