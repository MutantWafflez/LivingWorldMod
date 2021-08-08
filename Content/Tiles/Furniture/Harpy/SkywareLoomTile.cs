using LivingWorldMod.Content.Items.Placeables.Furniture.Harpy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Furniture.Harpy {

    public class SkywareLoomTile : BaseTile {

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

            ModTranslation mapName = CreateMapEntryName();
            mapName.SetDefault("Skyware Loom");

            AddMapEntry(Color.FloralWhite);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Item.NewItem(i * 16, j * 16, 32, 48, ModContent.ItemType<SkywareLoomItem>());
        }
    }
}