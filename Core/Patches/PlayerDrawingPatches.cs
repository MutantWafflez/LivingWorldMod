using System;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Custom.Utilities;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.Patches {
    /// <summary>
    /// "Patches" sort of class that handles the loading and injecting of patches
    /// related to player drawing.
    /// </summary>
    public class PlayerDrawingPatches : ILoadable {
        private delegate void RefAction(ref PlayerDrawSet drawInfo);

        private readonly Item _fakeItem = new Item();

        public void Load(Mod mod) {
            IL.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayerInternal += ModifyScaleDuringDoorOpening;
        }

        public void Unload() { }

        private void ModifyPlayerSize(ref PlayerDrawSet drawInfo) {
            PyramidDoorSystem doorSystem = ModContent.GetInstance<PyramidDoorSystem>();

            if (doorSystem.DoorOpeningPhase == 5) {
                drawInfo.heldItem = _fakeItem;
                PlayerDrawLayers.DrawPlayer_ScaleDrawData(ref drawInfo, 1f - doorSystem.DoorOpeningTimer / 240f / 3f);
            }
        }

        private void ModifyScaleDuringDoorOpening(ILContext il) {
            //Relatively simple patch; there are some shenanigans with draw order where using ModPlayer.ModifyDrawInfo and changing scale there doesn't do anything, so we are going straight to the source

            ILCursor c = new ILCursor(il);

            byte drawInfoVarNum = 0;

            //Jump to right before normal scale call, then emit our own call for modifying player size
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchCall(typeof(PlayerDrawLayers), nameof(PlayerDrawLayers.DrawPlayer_TransformDrawData)));
            c.Emit(OpCodes.Ldloca_S, drawInfoVarNum);
            c.EmitDelegate<RefAction>(ModifyPlayerSize);
        }
    }
}