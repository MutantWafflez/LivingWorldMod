using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.NPCs
{
	// Party Zombie is a pretty basic clone of a vanilla NPC. To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class GiantFrog : ModNPC
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Giant Frog");
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.Derpling];
		}

		public override void SetDefaults() {
			npc.width = 18;
			npc.height = 13;
			npc.scale = Main.rand.NextFloat(0.8f, 1.2f);
			npc.damage = (int)(10 * npc.scale);
			npc.defense = 6;
			npc.lifeMax = (int)(40 * npc.scale);
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.value = 60f;
			npc.knockBackResist = 0;
			npc.aiStyle = 41;
			animationType = NPCID.Derpling;
		}

		public override void NPCLoot()
		{
			if (Main.rand.Next(10) == 0) {
				Item.NewItem(npc.getRect(), ItemID.FrogLeg);
			}
		}

		public override void HitEffect(int hitDirection, double damage) {
			if (npc.life <= 0) {
				for (int i = 0; i < 6; i++) {
					int dust = Dust.NewDust(npc.position, npc.width, npc.height, 200, 2 * hitDirection, -2f);
					if (Main.rand.NextBool(2)) {
						Main.dust[dust].noGravity = true;
						Main.dust[dust].scale = 1.2f * npc.scale;
					}
					else {
						Main.dust[dust].scale = 0.7f * npc.scale;
					}
				}
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/FrogHead"), npc.scale);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/FrogLeg"), npc.scale);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/FrogBody"), npc.scale);
			}
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.SurfaceJungle.Chance * 0.8f;
		}
	}
}
