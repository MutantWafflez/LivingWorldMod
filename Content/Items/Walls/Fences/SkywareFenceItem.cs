using LivingWorldMod.Content.Walls.Fences;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace LivingWorldMod.Content.Items.Walls.Fences {
    public class SkywareFenceItem : BaseItem {
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DirtWall);
            Item.placeStyle = 0;
            Item.value = Item.buyPrice(copper: 25);
            Item.createWall = ModContent.WallType<SkywareFenceWall>();
        }
    }
}
