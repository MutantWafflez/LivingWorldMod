﻿using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Subworlds;
using LivingWorldMod.Core.PacketHandlers;
using LivingWorldMod.Custom.Classes.Cutscenes;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Interactables {
    /// <summary>
    /// Not an actual door in a traditional sense; it looks like one, but right clicking doesn't
    /// actually change the tile itself. Allows entrance into the Revamped Pyramid Subworld.
    /// </summary>
    public class PyramidDoorTile : BaseTile {
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
            //TODO: Multiplayer compat?
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j);
            CutscenePlayer cutscenePlayer = Main.LocalPlayer.GetModPlayer<CutscenePlayer>();

            if (cutscenePlayer.CurrentCutscene is EnterPyramidCutscene cutscene && cutscene.DoorBeingOpenedPosition == topLeft) {
                frameYOffset = (int)MathHelper.Clamp(cutscene.DoorAnimationPhase - 1, 0f, EnterPyramidCutscene.LastDoorAnimationPhase) * 72;
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
            Point16 topLeft = TileUtils.GetTopLeftOfMultiTile(Framing.GetTileSafely(i, j), i, j);

            cutscenePlayer.StartCutscene(new EnterPyramidCutscene(topLeft));
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                ModPacket packet = ModContent.GetInstance<CutscenePacketHandler>().GetPacket(CutscenePacketHandler.SyncPyramidEnterCutscene);
                packet.Write(topLeft.X);
                packet.Write(topLeft.Y);

                packet.Send();
            }

            return true;
        }
    }
}