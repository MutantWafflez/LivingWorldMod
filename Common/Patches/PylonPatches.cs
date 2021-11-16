using System;
using LivingWorldMod.Common.Systems;
using Terraria;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Utilities;
using Mono.Cecil.Cil;
using Terraria.ModLoader;
using MonoMod.Cil;

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

        private bool PlayerNearPylon(On.Terraria.GameContent.TeleportPylonsSystem.orig_IsPlayerNearAPylon orig, Terraria.Player player) {
            // Count waystones as "pylons" for teleportation
            return player.IsTileTypeInInteractionRange(ModContent.TileType<WaystoneTile>()) || orig(player);
        }

        private void IsInInteractionRange(ILContext il) {
            // Simple edit here. Just need to add Waystone tiles to the Interaction check, by navigating to where
            // the tile is checked for being a pylon, and also check if said tile is a waystone or not.

            ILCursor c = new ILCursor(il);

            byte isPylonLocalNumber = 3;

            // Navigate to the instruction after the stack allocation of if the tile is a pylon or not
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchStloc(isPylonLocalNumber));

            // Load normal value onto stack
            c.Emit(OpCodes.Ldloc_3);
            // Load current tile onto stack
            c.Emit(OpCodes.Ldloc_2);
            //If the variable is already true, then just return itself; but if not, check if tile is waystone
            c.EmitDelegate<Func<bool, Tile, bool>>((isPylon, currentTile) => isPylon || currentTile.type == ModContent.TileType<WaystoneTile>());
            // Allocate value to variable
            c.Emit(OpCodes.Stloc_3);

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

            //Navigate to the beginning of the teleportation initiation code
            c.ErrorOnFailedGotoNext(i => i.MatchStloc(34));

            //Move to proper instruction location
            c.Index--;
            //Mark label, and then reset instruction location
            exitInstruction = c.MarkLabel();
            c.Index = 0;

            //IL code for above block:
            /*
            0x005EE3BD 08           IL_02AD: ldloc.2
            0x005EE3BE 1322         IL_02AE: stloc.s V_34
            */

            //Navigate to a couple instructions after the pylon iteration loop
            c.ErrorOnFailedGotoNext(i => i.MatchStloc(31));

            //Move to RIGHT after the loop
            c.Index -= 3;
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

            //IL code for above block:
            /*
                0x005EE37F 1113         IL_026F: ldloc.s   flag2
                0x005EE381 16           IL_0271: ldc.i4.0
                0x005EE382 FE01         IL_0272: ceq
                0x005EE384 131F         IL_0274: stloc.s   V_31
            */
        }
    }
}
