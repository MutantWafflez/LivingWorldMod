using System;
using System.Collections.Generic;
using LivingWorldMod.Core.PacketHandlers;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players {
    /// <summary>
    /// ModPlayer that handles the accessories in this mod.
    /// </summary>
    public class AccessoryPlayer : ModPlayer {
        /// <summary>
        /// Nested class that functions as an instance that can be
        /// temporary or permanent (until death) for disabling a
        /// certain accessory or accessory slot.
        /// </summary>
        public class DisabledAccessoryInstance {
            public readonly bool persistent;
            public readonly int typeOrSlot;

            public int durationTimer;

            public DisabledAccessoryInstance(int durationTimer, bool persistent, int typeOrSlot) {
                this.durationTimer = durationTimer;
                this.persistent = persistent;
                this.typeOrSlot = typeOrSlot;
            }

            public override bool Equals(object obj) => obj is DisabledAccessoryInstance instance && typeOrSlot == instance.typeOrSlot && persistent == instance.persistent;

            public override int GetHashCode() => HashCode.Combine(persistent, typeOrSlot);
        }

        /// <summary>
        /// A list of instances that describe which types of items are disabled, i.e they cannot be unequipped
        /// and provide no effects.
        /// </summary>
        public List<DisabledAccessoryInstance> disabledAccessoryTypes = new();

        /// <summary>
        /// A list of instances that describe which accessory slots are disabled, i.e any item within the slot
        /// doesn't grant any effects, and cannot manipulate the slot at all (no equipping or un-equipping).
        /// </summary>
        public List<DisabledAccessoryInstance> disabledAccessorySlots = new();

        public override void ResetEffects() {
            bool RemoveCheck(DisabledAccessoryInstance instance) {
                return !instance.persistent && --instance.durationTimer <= 0;
            }

            disabledAccessoryTypes.RemoveAll(RemoveCheck);
            disabledAccessorySlots.RemoveAll(RemoveCheck);
        }

        public override void UpdateDead() {
            disabledAccessoryTypes.Clear();
            disabledAccessorySlots.Clear();
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
            ModPacket packet = ModContent.GetInstance<AccessoryPacketHandler>().GetPacket(AccessoryPacketHandler.SyncAccessoryPlayer);
            packet.Write(Player.whoAmI);
            int disabledTypesCount = disabledAccessoryTypes.Count;
            int disabledSlotsCount = disabledAccessorySlots.Count;

            packet.Write(disabledTypesCount);
            for (int i = 0; i < disabledTypesCount; i++) {
                AccessoryPacketHandler.WriteDisabledAccessoryInstance(disabledAccessoryTypes[i], packet);
            }

            packet.Write(disabledSlotsCount);
            for (int i = 0; i < disabledTypesCount; i++) {
                AccessoryPacketHandler.WriteDisabledAccessoryInstance(disabledAccessorySlots[i], packet);
            }

            packet.Send(toWho, fromWho);
        }
    }
}