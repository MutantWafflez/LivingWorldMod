using LivingWorldMod.NPCs.Villagers;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod
{
    public class LWMWorld : ModWorld
    {
        public static int[] villageReputation = new int[(int)VillagerType.VillagerTypeCount];

        /// <summary>
        /// Changes the inputted VillagerType's reputation by amount.
        /// </summary>
        /// <param name="villagerType">Enum of VillagerType</param>
        /// <param name="amount">Amount by which the reputation is changed</param>
        public static void ModifyReputation(VillagerType villagerType, int amount)
        {
            if (villagerType >= 0 && villagerType < VillagerType.VillagerTypeCount)
            {
                villageReputation[(int)villagerType] += amount;
            }
        }

        /// <summary>
        /// Changes the inputted VillagerType's reputation by amount, and creates a combat text by the changed amount at combatTextPosition.
        /// </summary>
        /// <param name="villagerType">Enum of VillagerType</param>
        /// <param name="amount">Amount by which the reputation is changed</param>
        /// <param name="combatTextPosition">Location of the combat text created to signify the changed reputation amount</param>
        public static void ModifyReputation(VillagerType villagerType, int amount, Rectangle combatTextPosition)
        {
            if (villagerType >= 0 && villagerType < VillagerType.VillagerTypeCount)
            {
                villageReputation[(int)villagerType] += amount;

                Color combatTextColor = new Color(255, 0, 0);
                if (amount > 0)
                    combatTextColor = new Color(0, 255, 0);
                CombatText.NewText(combatTextPosition, combatTextColor, (amount > 0 ? "+" : "") + amount + " Reputation", true);
            }
        }

        public override TagCompound Save()
        {
            return new TagCompound {
                {"VillageReputation", villageReputation }
            };
        }

        public override void Load(TagCompound tag)
        {
            villageReputation = tag.GetIntArray("VillageReputation");
        }
    }
}
