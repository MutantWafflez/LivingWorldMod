using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System.Collections.Generic;

namespace LivingWorldMod.NPCs.Villagers
{
    public abstract class Villager : ModNPC
    {
        public static readonly string VILLAGER_SPRITE_PATH = nameof(LivingWorldMod) + "/Textures/NPCs/Villagers/";

        public readonly List<string> possibleNames;

        public bool isHatedRep;
        public bool isNegativeRep;
        public bool isNeutralRep = true; //This is set to true prematurely *just* in case UpdateReputationBools() isn't called.
        public bool isPositiveRep;
        public bool isMaxRep;

        public Vector2 homePosition;

        public bool isMerchant = true;

        public VillagerType villagerType;

        public int spriteVariation = 0;

        public string VillagerName
        {
            get
            {
                return villagerType.ToString();
            }
        }

        public Villager()
        {
            possibleNames = GetPossibleNames();
        }

        public override string Texture => VILLAGER_SPRITE_PATH + VillagerName + "Style1";

        public override string[] AltTextures => new string[] { 
            VILLAGER_SPRITE_PATH + VillagerName + "Style2",
            VILLAGER_SPRITE_PATH + VillagerName + "Style3"
        };

        #region Defaults Methods
        public override bool CloneNewInstances => true;

        public override ModNPC Clone()
        {
            Villager clonedNPC = (Villager)base.Clone();
            clonedNPC.spriteVariation = Main.rand.Next(0, 3);
            return clonedNPC;
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.ExtraTextureCount[npc.type] = 2;
        }

        public override void SetDefaults()
        {
            npc.width = 25;
            npc.height = 40;
            npc.friendly = true;
            npc.lifeMax = 500;
            npc.defense = 15;
            npc.knockBackResist = 0.5f;
            npc.aiStyle = 7;
            animationType = NPCID.Guide;
        }
        #endregion

        #region Update Methods
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D textureToDraw;
            if (spriteVariation == 0)
            {
                textureToDraw = Main.npcTexture[npc.type];
            }
            else
            {
                textureToDraw = Main.npcAltTextures[npc.type][spriteVariation];
            }
            SpriteEffects spriteDirection = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(textureToDraw, new Rectangle((int)(npc.Right.X - (npc.frame.Width / 1.5) - Main.screenPosition.X), (int)(npc.Bottom.Y - npc.frame.Height - Main.screenPosition.Y + 2f), npc.frame.Width, npc.frame.Height), npc.frame, drawColor, npc.rotation, default(Vector2), spriteDirection, 0);
            return false;
        }

        public override bool CheckActive() => false;

        public override void PostAI()
        {
            UpdateReputationBools();
        }
        #endregion

        #region Chat Methods
        public override bool CanChat() => true;

        public override string GetChat() => GetDialogueText();

        public override string TownNPCName() => possibleNames[WorldGen.genRand.Next(possibleNames.Count)];

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
                Main.npcChatText = GetReputationText();
            }
            else if (!firstButton && isMerchant)
            {
                Main.npcChatText = GetReputationText();
            }
            else
            {
                shop = true;
            }
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Method used to determine what is said to the player upon right click.
        /// </summary>
        /// <returns>Returns a value telling the player to contact a mod dev by default.</returns>
        public virtual WeightedRandom<string> GetDialogueText()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();
            chat.Add("If someone saw this text... I'd be scared and tell a mod dev immediately!");
            return chat;
        }

        /// <summary>
        /// Method used to determine what is said to the player based on the Village reputation upon pressing the Reputation button.
        /// </summary>
        /// <returns>Returns a value telling the player to contact a mod dev by default.</returns>
        public virtual WeightedRandom<string> GetReputationText()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();
            chat.Add("If someone saw this text... I'd be scared and tell a mod dev immediately!");
            return chat;
        }

        public virtual List<string> GetPossibleNames()
        {
            List<string> names = new List<string>();
            names.Add("Villager (Report to Mod Dev!)");
            return names;
        }
        #endregion

        #region Miscellaneous Methods
        private void UpdateReputationBools()
        {
            float reputation = LWMWorld.GetReputation(villagerType);
            if (reputation <= 0f) {
                isHatedRep = true;
                isNegativeRep = false;
                isNeutralRep = false;
                isPositiveRep = false;
                isMaxRep = false;
            }
            else if (reputation <= 50f && reputation > 0f)
            {
                isHatedRep = false;
                isNegativeRep = true;
                isNeutralRep = false;
                isPositiveRep = false;
                isMaxRep = false;
            }
            else if (reputation >= 100f && reputation <= 150f)
            {
                isHatedRep = false;
                isNegativeRep = false;
                isNeutralRep = true;
                isPositiveRep = false;
                isMaxRep = false;
            }
            else if (reputation > 150f && reputation < 200f)
            {
                isHatedRep = false;
                isNegativeRep = false;
                isNeutralRep = false;
                isPositiveRep = true;
                isMaxRep = false;
            }
            else if (reputation >= 200f)
            {
                isHatedRep = false;
                isNegativeRep = false;
                isNeutralRep = false;
                isPositiveRep = false;
                isMaxRep = true;
            }
        }
        #endregion
    }
}