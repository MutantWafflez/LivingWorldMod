using LivingWorldMod.Items.Debug;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Tiles.Interactables
{
    class HarpyShrine_Tile : ModTile
    {
		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;

			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16};
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Harpy Shrine");
			AddMapEntry(new Color(255, 170, 0), name);
			dustType = DustID.Gold;
			disableSmartCursor = true;

			animationFrameHeight = 90;
		}
		public override bool HasSmartInteract()
		{
			return true;
		}
		public override bool NewRightClick(int i, int j)
        {
            return true;
        }
		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			if (++frameCounter >= 10)
			{
				frameCounter = 0;
				frame = ++frame % 16;
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 32, 48, ItemType<HarpyShrine>());
		}
	}
}
