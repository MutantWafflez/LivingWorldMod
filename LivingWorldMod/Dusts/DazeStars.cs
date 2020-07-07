using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Dusts
{
	public class DazeStars : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.velocity *= 2;
			dust.noGravity = true;
			dust.noLight = true;
			dust.scale *= 2;
		}

		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			if(Main.GameUpdateCount % 90 == 0) {
				dust.active = false;
			}
			return false;
		}
	}
}