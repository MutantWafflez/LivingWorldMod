using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace LivingWorldMod.NPCs.Villagers
{
    public abstract class Villager : ModNPC
    {
        public static readonly string VILLAGER_SPRITE_PATH = nameof(LivingWorldMod) + "/NPCs/Villagers/Textures/";

        public VillagerType villagerType;

        public int spriteVariation = -1;

        public string VillagerName
        {
            get
            {
                return villagerType.ToString();
            }
        }

        public override string Texture => VILLAGER_SPRITE_PATH + VillagerName + "Style1";

        public override string[] AltTextures => new string[] { 
            VILLAGER_SPRITE_PATH + VillagerName + "Style1",
            VILLAGER_SPRITE_PATH + VillagerName + "Style2",
            VILLAGER_SPRITE_PATH + VillagerName + "Style3"
        };

        public override bool CloneNewInstances => true;

        public override ModNPC Clone()
        {
            Villager clonedNPC = (Villager)base.Clone();
            if (spriteVariation == -1)
            {
                spriteVariation = Main.rand.Next(1, 4);
            }
            if (clonedNPC.spriteVariation == -1)
            {
                clonedNPC.spriteVariation = Main.rand.Next(1, 4);
            }
            return clonedNPC;
        }

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 40;
            npc.friendly = true;
            npc.lifeMax = 500;
            npc.defense = 15;
            npc.knockBackResist = 0.5f;
            npc.aiStyle = 7;
            animationType = NPCID.Guide;
        }

        //TODO: Figure how in the heck to draw NPCs manually properly
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D realTexture = Main.npcAltTextures[npc.type][spriteVariation - 1];
            spriteBatch.Draw(realTexture, npc.getRect(), npc.frame, drawColor, 0, default(Vector2), SpriteEffects.None, 0f);
            return true;
        }

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