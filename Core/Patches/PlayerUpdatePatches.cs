using System;
using System.Linq;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Subworlds.Pyramid;
using LivingWorldMod.Core.PacketHandlers;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using On.Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Collision = Terraria.Collision;
using Item = Terraria.Item;
using Main = Terraria.Main;

namespace LivingWorldMod.Core.Patches {
    /// <summary>
    /// ILoadable class that holds patches for specifically the Update method for
    /// players and any tertiary related methods.
    /// </summary>
    public class PlayerUpdatePatches : ILoadable {
        public void Load(Mod mod) {
            Player.JumpMovement += PlayerGravityManipulation;
            Player.VanillaUpdateEquip += DisableEquipFunctionality;
            Player.ApplyEquipFunctional += DisableAccessoryFunctionality;
            Player.IsAValidEquipmentSlotForIteration += DisableAccessySlotFunctionality;

            IL.Terraria.Player.Update += ForceWaterPhysics;
            IL.Terraria.Player.StickyMovement += ForceStickyPhysics;
        }

        public void Unload() { }

        private void PlayerGravityManipulation(Player.orig_JumpMovement orig, Terraria.Player self) {
            //Due to the limitations of ModPlayer update hooks, we must mess with gravity here.
            PyramidDungeonPlayer player = self.GetModPlayer<PyramidDungeonPlayer>();
            foreach (PyramidRoomCurseType curse in player.CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.GravitationalInstability:
                        self.gravDir = player.forcedGravityDirection;

                        if (Main.netMode != NetmodeID.Server && self.whoAmI == Main.myPlayer && --player.gravitySwapTimer <= 0) {
                            player.forcedGravityDirection *= -1;
                            player.gravitySwapTimer = Main.rand.Next(60 * 5, 60 * 10);

                            if (Main.netMode == NetmodeID.MultiplayerClient) {
                                ModPacket packet = ModContent.GetInstance<PyramidDungeonPacketHandler>().GetPacket(PyramidDungeonPacketHandler.SyncPlayerGravitySwap);
                                packet.Write(player.forcedGravityDirection);

                                packet.Send();
                            }
                        }

                        break;
                }
            }

            orig(self);
        }

        private void DisableEquipFunctionality(Player.orig_VanillaUpdateEquip orig, Terraria.Player self, Item item) {
            //Small detour that will properly disable armor/accessories if they are denoted to be disabled by the AccessoryPlayer class.
            if (self.GetModPlayer<AccessoryPlayer>().disabledAccessoryTypes.Any(instance => instance.typeOrSlot == item.type)) {
                return;
            }

            orig(self, item);
        }

        private void DisableAccessoryFunctionality(Player.orig_ApplyEquipFunctional orig, Terraria.Player self, Item currentItem, bool hideVisual) {
            //Small detour that will properly disable armor/accessories if they are denoted to be disabled by the AccessoryPlayer class.
            if (self.GetModPlayer<AccessoryPlayer>().disabledAccessoryTypes.Any(instance => instance.typeOrSlot == currentItem.type)) {
                return;
            }

            orig(self, currentItem, hideVisual);
        }

        private bool DisableAccessySlotFunctionality(Player.orig_IsAValidEquipmentSlotForIteration orig, Terraria.Player self, int slot) {
            return orig(self, slot) && self.GetModPlayer<AccessoryPlayer>().disabledAccessorySlots.Any(instance => instance.typeOrSlot == slot);
        }

        private void ForceWaterPhysics(ILContext il) {
            //Small edit that will force the physical effects of being underwater, regardless of whether or not
            //the player is actually submerged.

            ILCursor c = new(il);

            //Navigate to WetCollision check
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchCall<Collision>(nameof(Collision.WetCollision)));
            //Add our own check
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, Terraria.Player, bool>>((originalValue, player) => player.GetModPlayer<PyramidDungeonPlayer>().CurrentCurses.Contains(PyramidRoomCurseType.Viscosity) || originalValue);
        }

        private void ForceStickyPhysics(ILContext il) {
            //Small edit that will force the physical effects of touching a honey block, regardless of whether or not
            //the player is actually touching a honey block. Player must be colliding with a block for this to apply.

            ILCursor c = new(il);

            //Navigate to Sticky Tile call.
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchCall<Collision>(nameof(Collision.StickyTiles)));
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Terraria.Player, Vector2>>(player => {
                if (player.GetModPlayer<PyramidDungeonPlayer>().CurrentCurses.Contains(PyramidRoomCurseType.Adhesion)) {
                    return Collision.IsClearSpotTest(player.position + player.velocity, 1f, player.width, player.height, gravDir: (int)player.gravDir, checkSlopes: true) ? new Vector2(-1f) : new Vector2(0f);
                }

                return Collision.StickyTiles(player.position, player.velocity, player.width, player.height);
            });
        }
    }
}