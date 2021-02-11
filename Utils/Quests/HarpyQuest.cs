using LivingWorldMod.NPCs.Villagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace LivingWorldMod.Utilities.Quests
{
    public class HarpyQuest : VillagerQuest
    {
        public readonly int requiredItemID;

        public override VillagerType PertainedVillager => VillagerType.Harpy;

        public HarpyQuest(int questItem, string questDialogue)
        {
            requiredItemID = questItem;
            questText = questDialogue;
        }

        public override bool ActivationCondition(Player player, NPC npc)
        {
            if (player.HasItem(requiredItemID))
            {
                player.inventory.First(item => item.type == requiredItemID).stack--;
                return true;
            }

            return false;
        }
    }
}