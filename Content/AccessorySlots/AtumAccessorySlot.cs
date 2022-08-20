using System.Linq;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Items.Accessories.Boons;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.AccessorySlots {
    /// <summary>
    /// Accessory slot that only exists while the <seealso cref="AtumBoon"/> accessory
    /// is equipped.
    /// </summary>
    public class AtumAccessorySlot : ModAccessorySlot {
        public override bool DrawVanitySlot => false;

        public override bool DrawDyeSlot => false;

        public override string FunctionalBackgroundTexture => "Terraria/Images/Inventory_Back11";

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) => checkItem.ModItem is BoonAccessoryItem and not AtumBoon;

        public override bool IsEnabled() => Player.GetModPlayer<AccessoryPlayer>().activeAccessoryEffects[ModContent.ItemType<AtumBoon>()];

        public override void ApplyEquipEffects() {
            if (FunctionalItem.IsAir) {
                return;
            }

            (FunctionalItem.ModItem as BoonAccessoryItem)!.PotentBoonUpdate(Player);
        }
    }
}