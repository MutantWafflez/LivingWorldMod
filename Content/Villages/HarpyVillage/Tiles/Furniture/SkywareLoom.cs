using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.Tiles;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;

public class SkywareLoomTile : BaseTile {
    public override Color? TileColorOnMap => Color.FloralWhite;

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = false;
        Main.tileNoSunLight[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileWaterDeath[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.Origin = new Point16(0, 2);
        TileObjectData.addTile(Type);

        AdjTiles = [TileID.Loom];
    }
}

public class SkywareLoomItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.LivingLoom);
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(silver: 40);
        Item.createTile = ModContent.TileType<SkywareLoomTile>();
    }
}