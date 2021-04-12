using System.Linq;
using LivingWorldMod.Custom.Enums;
using Terraria;

namespace LivingWorldMod.Custom.Classes.Quests
{
    public class HarpyQuest : VillagerQuest
    {
        public readonly int requiredItemID;

        public override VillagerID PertainedVillager => VillagerID.Harpy;

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