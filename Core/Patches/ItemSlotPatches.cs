using System.Linq;
using LivingWorldMod.Common.Players;
using On.Terraria.UI;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.Patches {
    public class ItemSlotPatches : ILoadable {
        public void Load(Mod mod) {
            ItemSlot.isEquipLocked += ExpandedEquipLock;
        }

        public void Unload() { }

        private bool ExpandedEquipLock(ItemSlot.orig_isEquipLocked orig, int type) {
            //"Revamped" isEquipLocked for additional and adaptable functionality.
            bool origValue = orig(type);
            AccessoryPlayer player = Main.LocalPlayer.GetModPlayer<AccessoryPlayer>();

            return origValue
                   || player.disabledAccessoryTypes.Any(instance => instance.typeOrSlot == type)
                   || player.disabledAccessorySlots.Any(instance => Main.LocalPlayer.armor[instance.typeOrSlot].type == type);
        }
    }
}