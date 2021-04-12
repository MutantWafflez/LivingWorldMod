using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Materials {

    public class SkyBudItem : ModItem {

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sky Bud");
            Tooltip.SetDefault("'The petals feel like the clouds'");
        }

        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Daybloom);
            item.value = Item.sellPrice(silver: 10);
            item.rare = ItemRarityID.Green;
        }
    }
}