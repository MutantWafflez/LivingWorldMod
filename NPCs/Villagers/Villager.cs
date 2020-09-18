using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.Utilities;

namespace LivingWorldMod.NPCs.Villagers
{
    public abstract class Villager : ModNPC
    {


        /*public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 40;
            npc.friendly = true;
            npc.lifeMax = 500;
            npc.defense = 15;
            npc.knockBackResist = 0.5f;
            npc.aiStyle = 7;
        }*/

        /*public override void SetChatButtons(ref string button, ref string button2)
        {
            if (isMerchant)
            {
                button = Language.GetTextValue("LegacyInterface.28");
                button2 = "Reputation";
            }
            else
            {
                button = "Reputation";
            }
        }*/

        /*public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (firstButton && isMerchant)
            {
                shop = true;
            }
            else if (firstButton && !isMerchant)
            {
                Main.npcChatText = GetReputationChat();
            }
            else if (!firstButton && isMerchant)
            {
                Main.npcChatText = GetReputationChat();
            }
            else
            {
                shop = true;
            }
        }*/

        /*private void UpdateReputationBools()
        {
            float reputation = LWMWorld.villageReputation[(int)villagerType];
            if (reputation < -30f)
            {
                isNegativeRep = true;
                isNeutralRep = false;
                isPositiveRep = false;
                isMaxRep = false;
            }
            else if (reputation >= -30f && reputation <= 30f)
            {
                isNegativeRep = false;
                isNeutralRep = true;
                isPositiveRep = false;
                isMaxRep = false;
            }
            else if (reputation > 30f && reputation < 100f)
            {
                isNegativeRep = false;
                isNeutralRep = false;
                isPositiveRep = true;
                isMaxRep = false;
            }
            else if (reputation >= 100f)
            {
                isNegativeRep = true;
                isNeutralRep = false;
                isPositiveRep = false;
                isMaxRep = true;
            }
        }
        */
    }
}