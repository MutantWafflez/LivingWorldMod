using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace LivingWorldMod.NPCs.Villagers
{
    public abstract class Villager : ModNPC
    {
        public const string VILLAGER_TEXTURE_PATH = "LivingWorldMod/NPCs/VillagerTextures/";

        public bool isMerchant;

        /// <summary>
        /// What type of villager this is. Is used for the Texture.
        /// </summary>
        public readonly string villagerTextureType;

        /// <summary>
        /// What numerical type of villager this is. Used for Reputation values.
        /// </summary>
        public readonly VillagerType villagerType;

        /// <summary>
        /// Reputation chat component. Hostile messages
        /// </summary>
        public readonly string[] hostileResponses;

        /// <summary>
        /// Reputation chat component. Neutral messages
        /// </summary>
        public readonly string[] neutralResponses;

        /// <summary>
        /// Reputation chat component. Friendly messages
        /// </summary>
        public readonly string[] friendlyResponses;

        /// <summary>
        /// Number of the sprite variation of said Villager. Used for Texture loading.
        /// </summary>
        public readonly int spriteVariation;

        public Villager(string vilTextType, int spriteVar, VillagerType vilType, string[] hostile, string[] neutral, string[] friendly)
        {
            villagerTextureType = vilTextType;
            spriteVariation = spriteVar;
            villagerType = vilType;
            hostileResponses = hostile;
            neutralResponses = neutral;
            friendlyResponses = friendly;
        }

        public override string Texture => VILLAGER_TEXTURE_PATH + villagerTextureType + "Type" + spriteVariation;

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 40;
            npc.townNPC = true;
            npc.friendly = true;
            npc.lifeMax = 500;
            npc.defense = 15;
            npc.knockBackResist = 0.5f;
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

        public override string GetChat()
        {
            return "Hmmm... Seems like the Child Class of this Villager didn't override GetChat() correctly! What a shame!";
        }

        /// <summary>
        /// Gives the reputation text of the given village. Returns a not very nice message by default.
        /// </summary>
        public string GetReputationChat()
        {
            float reputation = LWMWorld.villageReputation[(int)villagerType];
            if (reputation < -30f)
            {
                return Main.rand.Next(hostileResponses);
            }
            else if (reputation >= -30f && reputation <= 30f)
            {
                return Main.rand.Next(neutralResponses);
            }
            else if (reputation > 30f)
            {
                return Main.rand.Next(friendlyResponses);
            }
            return "Reputation chat error! Report to devs! Reputation value: " + reputation;
        }
    }
}