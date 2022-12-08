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

            return origValue || Main.LocalPlayer.GetModPlayer<AccessoryPlayer>().disabledAccessoryTypes.Any(instance => instance.typeOrSlot == type);
        }
    }
}