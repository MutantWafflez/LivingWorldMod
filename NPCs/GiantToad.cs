using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.NPCs
{
	// Party Zombie is a pretty basic clone of a vanilla NPC. To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class GiantToad : ModNPC
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Giant Toad");
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.Derpling];
		}

		public override void SetDefaults() {
			npc.width = 18;
			npc.height = 13;
			npc.scale = Main.rand.NextFloat(1, 1.4f);
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
			if (Main.rand.Next(150) == 0) {
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
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/ToadHead"), npc.scale);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/ToadLeg"), npc.scale);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/ToadBody"), npc.scale);
			}
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.Underground.Chance * 0.1f;
		}
	}
}
