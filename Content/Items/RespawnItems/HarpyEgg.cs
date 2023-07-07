using Terraria.ID;

namespace LivingWorldMod.Content.Items.RespawnItems {
    /// <summary>
    /// Item used for respawning Harpy Villagers at the Harpy Village shrine.
    /// </summary>
    public class HarpyEgg : BaseItem {
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults() {
            Item.width = 22;
            Item.height = 24;
            Item.maxStack = 999;
            Item.rare = ItemRarityID.Blue;
        }
    }
}