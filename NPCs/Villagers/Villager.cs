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
        public readonly string villagerType;

        /// <summary>
        /// Number of the sprite variation of said Villager. Used for Texture loading.
        /// </summary>
        public readonly int spriteVariation;

        public Villager(string vilType, int spriteVar)
        {
            villagerType = vilType;
            spriteVariation = spriteVar;
        }

        public override string Texture => VILLAGER_TEXTURE_PATH + villagerType + "Type" + spriteVariation;

        public override void SetDefaults()
        {
            npc.aiStyle = 7;
            npc.width = 18;
            npc.height = 40;
            npc.townNPC = true;
            npc.friendly = true;
            npc.lifeMax = 200;
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
        public virtual string GetReputationChat()
        {
            return "Want to know your reputation? Sorry, no can do. We haven't met you before. (Default Message, Report to devs!)";
        }
    }
}