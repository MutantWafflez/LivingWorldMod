using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs
{
	public class ExampleGlobalNPC : GlobalNPC
	{
		public override void NPCLoot(NPC npc)
		{
			if((npc.type == NPCID.Clown) && (Main.rand.Next(30) == 0))
			{
					Item.NewItem(npc.getRect(), mod.ItemType("ComedicallyLargeMallet"));
			}
		}
	}
}
