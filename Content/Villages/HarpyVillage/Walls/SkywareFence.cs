using LivingWorldMod.Content.Walls;
using LivingWorldMod.Globals.BaseTypes.Items;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Walls;

public class SkywareFenceWall : BaseWall {
    public override Color? WallColorOnMap => Color.LightBlue;

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;
        WallID.Sets.AllowsWind[Type] = true;
        WallID.Sets.Transparent[Type] = true;

        DustType = DustID.t_LivingWood;

        base.SetStaticDefaults();
    }
}

public class SkywareFenceItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 40;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.DirtWall);
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(copper: 25);
        Item.createWall = ModContent.WallType<SkywareFenceWall>();
    }
}