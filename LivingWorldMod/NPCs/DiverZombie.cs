using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.NPCs
{
	// Party Zombie is a pretty basic clone of a vanilla NPC. To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class DiverZombie : ModNPC
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Diver Zombie");
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.Zombie];
		}

		public override void SetDefaults() {
			npc.width = 34;
			npc.height = 48;
			npc.damage = 55;
			npc.defense = 28;
			npc.lifeMax = 260;
			npc.HitSound = SoundID.NPCHit4;
			npc.DeathSound = SoundID.NPCDeath2;
			npc.value = 60f;
			npc.knockBackResist = 0.5f;
			npc.aiStyle = 3;
			aiType = NPCID.Zombie;
			animationType = NPCID.Zombie;
			banner = Item.NPCtoBanner(NPCID.Zombie);
			bannerItem = Item.BannerToItem(banner);
		}

				public override void NPCLoot()
		{
			if (Main.rand.Next(100) == 0) {
				Item.NewItem(npc.getRect(), ItemID.DivingHelmet);
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if(spawnInfo.player.ZoneBeach) {
			return Main.hardMode ? SpawnCondition.OverworldNightMonster.Chance * 4f : 0;
			}
			return 0;
		}

		public override void HitEffect(int hitDirection, double damage) {
						for (int i = 0; i < 10; i++) {
					int dust = Dust.NewDust(npc.position, npc.width, npc.height, 5, 2 * hitDirection, -2f);
				}
			if (npc.life <= 0) {
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/DiverHead"), npc.scale);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/DiverLeg"), npc.scale);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/DiverHand"), npc.scale);
			}
		}
	}
}
