using LivingWorldMod.Content.Walls.DebugWalls;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.DebugItems {
    public class SkipWallItem : DebugItem {
        public override string Texture => "Terraria/Images/Item_" + ItemID.StarsWallpaper;

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.StarsWallpaper);
            Item.maxStack = 1;
            Item.consumable = false;
            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.createWall = ModContent.WallType<SkipWall>();
        }
    }
}