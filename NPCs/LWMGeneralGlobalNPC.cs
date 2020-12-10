using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.NPCs
{
    public class LWMGeneralGlobalNPC : GlobalNPC
    {
        /// <summary>
        /// Prevents hostile harpies or wyverns from hitting Harpy Villagers.
        /// </summary>
        public override bool? CanHitNPC(NPC npc, NPC target)
        {
            if ((npc.type == NPCID.Harpy ||
                npc.type == NPCID.WyvernHead ||
                npc.type == NPCID.WyvernBody ||
                npc.type == NPCID.WyvernBody2 ||
                npc.type == NPCID.WyvernBody3 ||
                npc.type == NPCID.WyvernLegs ||
                npc.type == NPCID.WyvernTail)
                && target.type == ModContent.NPCType<SkyVillager>())
            {
                return false;
            }
            return base.CanHitNPC(npc, target);
        }

        public override void PostAI(NPC npc)
        {
            //Prevent any Hostile Harpies from being near the Harpy Village
            if (npc.type == NPCID.Harpy)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && npc.Distance(npc.FindNearestNPC(ModContent.NPCType<SkyVillager>()).Center) <= 16 * 30)
                {
                    npc.active = false;
                    npc.netUpdate = true;
                }
            }
        }
    }
}