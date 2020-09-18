using LivingWorldMod.NPCs.Villagers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.NPCs
{
    public class LWMGeneralGlobalNPC : GlobalNPC
    {
        /// <summary>
        /// Removes reputation from the Harpy Reputation value if a normal Harpy or Wyvern is killed
        /// </summary>
        public override bool SpecialNPCLoot(NPC npc)
        {
            switch (npc.type)
            {
                case NPCID.Harpy:
                    LWMWorld.ModifyReputation(VillagerType.SkyVillager, -1, new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height));
                    break;
                case NPCID.WyvernHead:
                    LWMWorld.ModifyReputation(VillagerType.SkyVillager, -5, new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height));
                    break;
            }
            return base.SpecialNPCLoot(npc);
        }


    }
}
