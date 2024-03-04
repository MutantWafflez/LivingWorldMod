using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Furniture.Harpy;

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

        AdjTiles = new int[] { TileID.Loom };
    }
}