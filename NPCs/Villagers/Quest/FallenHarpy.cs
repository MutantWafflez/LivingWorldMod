using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.NPCs.Villagers.Quest
{
    public class FallenHarpy : QuestVillager
    {
        public bool isUnconscious = false;

        private static readonly Texture2D UnconsciousTexture =
            ModContent.GetTexture("LivingWorldMod/NPCs/Villagers/Quest/FallenHarpy_Unconscious");

        public override WeightedRandom<string> InteractionDialogue
        {
            get
            {
                WeightedRandom<string> dialogue = new WeightedRandom<string>();
                dialogue.Add("It’s strange, I was flying through the sky when I saw a strange green beam of light in the distance. I tried flying after it, but it suddenly jolted and knocked into me. I lost consciousness soon after that.");
                dialogue.Add("The village has been pretty rowdy, all this talk about an awful human causing trouble. The nerve of them! I’m glad you’re better than that, right...?");
                dialogue.Add("Aw, my hair’s absolutely ruined, and my feathers are all ruffled up and-");
                dialogue.Add("I wonder if the pain’s gone yet. ...Nope, it’s definitely still there.");
                dialogue.Add("I’ve really got to get home, I bet all the other harpies are so worried right now!");
                return dialogue;
            }
        }

        public override string QuestCompletetionDialogue =>
            "Thank you so much for helping me, I can finally go back to the village. I’ll tell everyone about the good deed you did for me today. Goodbye human! Make sure you stop by the village sometime, I’m sure you can become great friends with the other harpies if you really try. Seeya around!";

        #region Defaults

        public override VillagerType VillagerType => VillagerType.Harpy;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 27;
        }

        #endregion Defaults

        #region Spawning Related

        public override int SpawnNPC(int tileX, int tileY)
        {
            int spawnValue = base.SpawnNPC(tileX, tileY);
            (Main.npc[spawnValue].modNPC as FallenHarpy).isUnconscious = true;
            return spawnValue;
        }

        #endregion Spawning Related

        #region AI/Drawing

        public override void AI()
        {
            if (isUnconscious)
            {
                npc.width = 60;
                npc.height = 22;
                npc.aiStyle = -1;
                npc.rarity = 4;
            }
            else
            {
                npc.width = 25;
                npc.height = 40;
                npc.aiStyle = 7;
                npc.rarity = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (isUnconscious)
            {
                Rectangle npcRect = npc.getRect();
                npcRect.X -= (int) Main.screenPosition.X;
                npcRect.Y -= (int) Main.screenPosition.Y;
                spriteBatch.Draw(UnconsciousTexture, npcRect, drawColor);
                return false;
            }

            return base.PreDraw(spriteBatch, drawColor);
        }

        #endregion AI/Drawing

        #region Chat

        public override string GetChat()
        {
            if (isUnconscious)
            {
                isUnconscious = false;
                npc.position.Y -= 22;
                return "Ow ow ow.. You’re asking me if it hurt when I fell from heaven? Of course it did! Thanks for helping me up at least.";
            }
            else
            {
                return InteractionDialogue;
            }
        }

        #endregion Chat
    }
}