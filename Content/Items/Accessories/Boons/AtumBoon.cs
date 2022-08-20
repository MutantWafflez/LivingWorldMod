using Terraria;

namespace LivingWorldMod.Content.Items.Accessories.Boons {
    /// <summary>
    /// Expert exclusive drop from the Pyramid Boss that grants
    /// an additional accessory slot for boons.
    /// </summary>
    public class AtumBoon : BoonAccessoryItem {
        public override string Texture => "Terraria/Images/NPC_Head_1";

        public override void SetDefaults() {
            Item.DefaultToAccessory();
            Item.expert = true;
            Item.expertOnly = true;
            Item.value = Item.sellPrice(gold: 5);
        }
    }
}