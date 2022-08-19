using Terraria.ID;

namespace LivingWorldMod.Content.Items.Accessories.Boons {
    /// <summary>
    /// Boon of Anubis, the one that does the death stuff.
    /// </summary>
    public class AnubisBoon : BoonAccessoryItem {
        //TODO: Get proper sprites for Boons!
        public override string Texture => "Terraria/Images/NPC_Head_38";

        public override void SetDefaults() {
            Item.DefaultToAccessory();
            Item.rare = ItemRarityID.Orange;
            Item.value = Terraria.Item.sellPrice(gold: 3);
        }
    }
}