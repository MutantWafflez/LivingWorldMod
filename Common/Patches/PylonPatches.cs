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
            IL.Terraria.Map.TeleportPylonsMapLayer.Draw += TeleportPylonsMapLayer_Draw;
        }

        private void TeleportPylonsMapLayer_Draw(ILContext il) {
            //Okay, so for some reason, when vanilla normally calls IsPlayerNearAPylon, it returns false even when near a Waystone
            //However, we can still teleport just fine, meaning that it understands that I actually am near a "pylon" (waystone),
            //but the map does not update accordingly. However, injecting the EXACT same code has it properly work? I have no idea
            //what is happening, but since this works, I'm gonna leave it here.
            ILCursor c = new ILCursor(il);

            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchStloc(4));

            c.Emit(OpCodes.Pop);
            c.EmitDelegate<Func<bool>>(() => {
                bool isNearPylon = TeleportPylonsSystem.IsPlayerNearAPylon(Main.LocalPlayer);
                return isNearPylon;
            });
        }

        public void Unload() { }

        private bool PlayerNearPylon(On.Terraria.GameContent.TeleportPylonsSystem.orig_IsPlayerNearAPylon orig, Player player) =>
            // Count waystones as "pylons" for teleportation
            player.IsTileTypeInInteractionRange(ModContent.TileType<WaystoneTile>()) || orig(player);

        private void IsInInteractionRange(ILContext il) {
            // Simple edit here. Just need to add Waystone tiles to the Interaction check, by navigating to where
            // the tile is checked for being a pylon, and also check if said tile is a waystone or not.

            ILCursor c = new ILCursor(il);

            ILLabel pylonTypeCheckBlock = c.DefineLabel();

            //Navigate to within the block that is determined by if the tile is a pylon tile or not
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchLdcI4(TileID.TeleportationPylon));
            c.Index++;

            pylonTypeCheckBlock = c.MarkLabel();

            //Move to right BEFORE that tile type == 597 check
            c.Index = 0;
            c.ErrorOnFailedGotoNext(i => i.MatchLdcI4(TileID.TeleportationPylon));
            c.Index -= 3;

            c.Emit(OpCodes.Ldloc_2);
            // Check if tile is a waystone
            c.EmitDelegate<Func<Tile, bool>>(currentTile => currentTile.TileType == ModContent.TileType<WaystoneTile>());
            // If true, move to the aforementioned stolen label that allows if block access
            c.Emit(OpCodes.Brtrue_S, pylonTypeCheckBlock);

            /*
            // IL block for above code ^:
            IL_0061: ldloca.s  tile
            IL_0063: call      instance uint16& Terraria.Tile::get_type()
            IL_0068: ldind.u2
            IL_0069: ldc.i4    597
            IL_006E: bne.un.s  IL_00E7

            IL_0070: ldarg.1
            IL_0071: ldloc.0
            IL_0072: bge.s     IL_0079
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
            c.ErrorOnFailedGotoNext(i => i.MatchLdloc(7));

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