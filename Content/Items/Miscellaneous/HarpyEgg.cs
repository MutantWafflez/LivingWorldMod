using Terraria.ID;

namespace LivingWorldMod.Content.Items.Miscellaneous {
    /// <summary>
    /// Item used for respawning Harpy Villagers at the Harpy Village shrine.
    /// </summary>
    public class HarpyEgg : BaseItem {
        public override void SetDefaults() {
            Item.width = 22;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
        }
    }
}