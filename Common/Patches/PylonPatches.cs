using System;
using LivingWorldMod.Common.Systems;
using Terraria;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Utilities;
using Mono.Cecil.Cil;
using Terraria.ModLoader;
using MonoMod.Cil;
using Terraria.GameContent;
using Terraria.ID;

namespace LivingWorldMod.Common.Patches {
    /// <summary>
    /// Class that handles patches within the realm of Pylons.
    /// </summary>
    public class PylonPatches : ILoadable {
        public void Load(Mod mod) {
            On.Terraria.GameContent.TeleportPylonsSystem.IsPlayerNearAPylon += PlayerNearPylon;
            IL.Terraria.Player.InInteractionRange += IsInInteractionRange;
            IL.Terraria.GameContent.TeleportPylonsSystem.HandleTeleportRequest += HandleTeleportRequest;
        }

        public void Unload() { }

        private bool PlayerNearPylon(On.Terraria.GameContent.TeleportPylonsSystem.orig_IsPlayerNearAPylon orig, Player player) =>
            // Count waystones as "pylons" for teleportation
            player.IsTileTypeInInteractionRange(ModContent.TileType<WaystoneTile>()) || orig(player);

        private void IsInInteractionRange(ILContext il) {
            // Simple edit here. Just need to add Waystone tiles to the Interaction check, by navigating to where
            // the tile is checked for being a pylon, and also check if said tile is a waystone or not.

            ILCursor c = new ILCursor(il);

            // Navigate right after the instruction that tests if the tile type is 597
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchLdcI4(TileID.TeleportationPylon));

            // Steal the label that is here that will allow access to the if block
            ILLabel trueTransformLabel = (ILLabel)c.Next.Operand;
            // Move fter this instruction, and add our own check
            c.Index++;
            c.Emit(OpCodes.Ldloc_2);
            // Check if tile is a waystone
            c.EmitDelegate<Func<Tile, bool>>(currentTile => currentTile.type == ModContent.TileType<WaystoneTile>());
            // If true, move to the aforementioned stolen label that allows if block access
            c.Emit(OpCodes.Brtrue_S, trueTransformLabel);

            // IL block for above code ^:
            /*
            /* 0x002FA2D3 7B020D0004   #1# IL_0063: ldfld     uint16 Terraria.Tile::'type'
	        /* 0x002FA2D8 20DB010000   #1# IL_0068: ldc.i4    475
	        /* 0x002FA2DD 2E0F         #1# IL_006D: beq.s     IL_007E

	        /* 0x002FA2DF 08           #1# IL_006F: ldloc.2
	        /* 0x002FA2E0 7B020D0004   #1# IL_0070: ldfld     uint16 Terraria.Tile::'type'
	        /* 0x002FA2E5 2055020000   #1# IL_0075: ldc.i4    597
	        /* 0x002FA2EA FE01         #1# IL_007A: ceq
	        /* 0x002FA2EC 2B01         #1# IL_007C: br.s      IL_007F

	        /* 0x002FA2EE 17           #1# IL_007E: ldc.i4.1

	        /* 0x002FA2EF 0D           #1# IL_007F: stloc.3
            */
        }

        private void HandleTeleportRequest(ILContext il) {
            // For this edit, we have to check over all waystone instances, and do some checks in order to trick the game
            // into thinking that the waystones are "pylons" so teleportation properly works with them. To do this, we will
            // jump into the heart of the method that handles teleportation, and iterate over all the waystones in the world,
            // then check if the player is near enough to one of them, and if so, initiate teleportation.

            ILCursor c = new ILCursor(il);

            ILLabel exitInstruction = c.DefineLabel();

            c.GotoLastInstruction();
            //Navigate to the beginning of the teleportation initiation code (which is at the end of the method)
            c.ErrorOnFailedGotoPrev(i => i.MatchLdloc(2));

            //Mark label, and then reset instruction location
            exitInstruction = c.MarkLabel();
            c.Index = 0;

            //IL code for above block:
            /*
            0x005EE3BD 08           IL_0213: ldloc.2
            0x005EE3BE 1322         IL_0214: brfalse IL_02ad
            */

            //Navigate to the end of the pylon iteration loop
            c.ErrorOnFailedGotoNext(i => i.MatchLdloc(6));

            //Also, we need the current player, which is the first local var, so we will grab that first
            c.Emit(OpCodes.Ldloc_0);
            //Then, add our own loop
            c.EmitDelegate<Func<Player, bool>>(player => {
                WaystoneSystem waystoneSystem = ModContent.GetInstance<WaystoneSystem>();

                for (int i = 0; i < waystoneSystem.waystoneData.Count; i++) {
                    WaystoneInfo currentInfo = waystoneSystem.waystoneData[i];
                    if (currentInfo.isActivated && player.IsInTileInteractionRange(currentInfo.tileLocation.X, currentInfo.tileLocation.Y)) {
                        return true;
                    }
                }

                return false;
            });
            // If our loop returns true, then move control to the exit instruction, which will initiate teleportation
            c.Emit(OpCodes.Brtrue_S, exitInstruction);
        }
    }
}