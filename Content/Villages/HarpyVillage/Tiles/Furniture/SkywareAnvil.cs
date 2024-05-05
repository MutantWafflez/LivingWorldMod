using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.Tiles;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;

public class SkywareAnvilTile : BaseTile {
    public override Color? TileColorOnMap => Color.LightBlue;

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = false;
        Main.tileNoSunLight[Type] = false;
        Main.tileSolidTop[Type] = true;
        Main.tileWaterDeath[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);

        AdjTiles = [TileID.Anvils];
    }
}

public class SkywareAnvilItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.IronAnvil);
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(silver: 5);
        Item.createTile = ModContent.TileType<SkywareAnvilTile>();
    }
}