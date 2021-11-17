using LivingWorldMod.Content.Tiles.Furniture.Critter;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeables.Furniture.Critter {
    public class NimbusJarItem : BaseItem {
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.BirdCage);
            Item.rare = ItemRarityID.Blue;
            Item.placeStyle = 0;
            Item.value = Terraria.Item.buyPrice(gold: 1);
            Item.createTile = ModContent.TileType<NimbusJarTile>();
        }
    }
}