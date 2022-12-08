using System;
using System.Collections.Generic;
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
        public readonly HashSet<DisabledAccessoryInstance> disabledAccessoryTypes = new();

        /// <summary>
        /// A list of instances that describe which accessory slots are disabled, i.e any item within the slot
        /// doesn't grant any effects, and cannot manipulate the slot at all (no equipping or un-equipping).
        /// </summary>
        public readonly HashSet<DisabledAccessoryInstance> disabledAccessorySlots = new();

        public override void ResetEffects() {
            disabledAccessoryTypes.RemoveWhere(instance => !instance.persistent && --instance.durationTimer <= 0);
            disabledAccessorySlots.RemoveWhere(instance => !instance.persistent && --instance.durationTimer <= 0);
        }

        public override void UpdateDead() {
            disabledAccessoryTypes.Clear();
            disabledAccessorySlots.Clear();
        }
    }
}