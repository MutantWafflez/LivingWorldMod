using System;
using Terraria;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Enums;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Tiles.WorldGen
{
    public class SpiderSacTile : ModTile
    {
        public override void SetDefaults()
        {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileWaterDeath[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.WaterDeath = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.addTile(Type);

			disableSmartCursor = true;

			ModTranslation mapName = CreateMapEntryName();
			mapName.SetDefault("Spider Sac");
			AddMapEntry(default(Color), mapName);
		}

        public override bool Dangersense(int i, int j, Player player) => true;
    }
}
