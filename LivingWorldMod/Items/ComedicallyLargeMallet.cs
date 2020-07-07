using LivingWorldMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Items
{
	public class ComedicallyLargeMallet : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Bonk!");
		}

		public override void SetDefaults() {
			item.damage = 55;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 30;
			item.useAnimation = 30;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = 7.5f;
			item.value = 75000;
			item.rare = ItemRarityID.Pink;
			item.UseSound = SoundID.Item1;
			item.scale = 1.2f;
		}
		// 
		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			if ((Main.rand.Next(2) == 0) && (!target.buffImmune[BuffID.Confused] && target.life >= 0)) {
				target.AddBuff(BuffID.Confused, 300);
								for (int d = 0; d < 5; d++)
				{
					Dust.NewDust(target.position, -1, -1, mod.DustType("DazeStars"));
				}
				Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/Bonk").WithVolume(3).WithPitchVariance(.5f));
				Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/Dizzybirds").WithPitchVariance(.5f));

			}
		}
	}
}
