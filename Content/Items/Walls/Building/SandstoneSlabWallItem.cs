using LivingWorldMod.Content.Walls.Building;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Walls.Building;

public class SandstoneSlabWallItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 40;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.StoneSlabWall);
        Item.createWall = ModContent.WallType<SandstoneSlabWall>();
    }
}