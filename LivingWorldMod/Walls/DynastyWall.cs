using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Walls
{
	public class DynastyWall : ModWall
	{
		public override void SetDefaults() {
			Main.wallHouse[Type] = true;
			dustType = (207);
			drop = ItemType<Items.Placeable.DynastyWall>();
			AddMapEntry(new Color(51, 26, 11));
		}
	}
}