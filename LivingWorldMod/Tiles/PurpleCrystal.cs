using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Tiles
{
	internal class PurpleCrystal : ModTile
	{
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileLighted[Type] = true;
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Purple Crystal");
			AddMapEntry(new Color(72, 22, 123), name);
			animationFrameHeight = 270;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.15f;
			g = 0;
			b = 0.25f;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			frameCounter++;
			if (++frameCounter >= 9)
			{
				frameCounter = 0;
				frame = ++frame % 4;
			}
		}
	}
}
