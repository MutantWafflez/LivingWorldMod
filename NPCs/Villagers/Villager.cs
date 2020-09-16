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
        public const string VILLAGER_TEXTURE_PATH = "LivingWorldMod/NPCs/VillagerTextures/";

        public bool isMerchant;

        public bool isNegativeRep;
        public bool isNeutralRep;
        public bool isPositiveRep;
        public bool isMaxRep;

        /// <summary>
        /// What type of villager this is. Is used for the Texture.
        /// </summary>
        public readonly string villagerTextureType;

        /// <summary>
        /// What numerical type of villager this is. Used for Reputation values.
        /// </summary>
        public readonly VillagerType villagerType;

        /// <summary>
        /// Normal GetChat() text for the given Villager.
        /// </summary>
        public WeightedRandom<string> getChat = new WeightedRandom<string>();

        /// <summary>
        /// Reputation Chat. Changes based on current Village's reputation.
        /// </summary>
        public WeightedRandom<string> reputationChat = new WeightedRandom<string>();

        /// <summary>
        /// Number of the sprite variation of said Villager. Used for Texture loading.
        /// </summary>
        public readonly int spriteVariation;

        public Villager(string vilTextType, int spriteVar, VillagerType vilType)
        {
            villagerTextureType = vilTextType;
            spriteVariation = spriteVar;
            villagerType = vilType;
        }

        public override string Texture => VILLAGER_TEXTURE_PATH + villagerTextureType + "Style" + spriteVariation;

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 40;
            npc.townNPC = true;
            npc.friendly = true;
            npc.lifeMax = 500;
            npc.defense = 15;
            npc.knockBackResist = 0.5f;
            npc.aiStyle = 7;
        }

        public override void SetChatButtons(ref string button, ref string button2)
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
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
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
        }

        public override bool CanChat() => true;

        public override void PostAI()
        {
            UpdateReputationBools();
            UpdateChatLists();
        }

        /// <summary>
        /// Gives the reputation text of the given village.
        /// </summary>
        public string GetReputationChat() => reputationChat;

        public override string GetChat() => getChat;

        private void UpdateReputationBools()
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

        public abstract void UpdateChatLists();
    }
}