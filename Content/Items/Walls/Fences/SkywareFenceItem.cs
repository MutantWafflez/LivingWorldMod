using LivingWorldMod.Content.Walls.Fences;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Walls.Fences {
    public class SkywareFenceItem : BaseItem {
        public override void SetStaticDefaults() {
            SacrificeTotal = 40;
        }

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DirtWall);
            Item.placeStyle = 0;
            Item.value = Item.buyPrice(copper: 25);
            Item.createWall = ModContent.WallType<SkywareFenceWall>();
        }
    }
}