using LivingWorldMod.Custom.Enums;
using Terraria;

namespace LivingWorldMod.Custom.Classes.Quests
{
    public abstract class VillagerQuest
    {
        /// <summary>
        /// What the villager will say when the "Quest" button is pressed for this specific quest.
        /// </summary>
        public string questText = "";

        /// <summary>
        /// What villager type this quest type pertains to.
        /// </summary>
        public virtual VillagerID PertainedVillager => VillagerID.VillagerTypeCount;

        /// <summary>
        /// Abstract method that will determine whether or not any given quest of this type will be fulfilled.
        /// </summary>
        /// <returns> Whether or not this quest's conditions has been satisfied. </returns>
        public abstract bool ActivationCondition(Player player, NPC npc);
    }
}