using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.NPCs.Villagers.Quest {
    public class FallenHarpy : QuestVillager {

        public bool isUnconscious = false;

        private static readonly Texture2D UnconsciousTexture = ModContent.GetTexture("LivingWorldMod/NPCs/Villagers/Quest/FallenHarpy_Unconscious");

        #region Defaults
        public override VillagerType VillagerType => VillagerType.Harpy;

        public override void SetStaticDefaults() {
            Main.npcFrameCount[npc.type] = 27;
        }
        #endregion

        #region Spawning Related
        public override int SpawnNPC(int tileX, int tileY) {
            int spawnValue = base.SpawnNPC(tileX, tileY);
            (Main.npc[spawnValue].modNPC as FallenHarpy).isUnconscious = true;
            return spawnValue;
        }
        #endregion

        #region AI/Drawing
        public override void AI() {
            if (isUnconscious) {
                npc.width = 60;
                npc.height = 22;
                npc.aiStyle = -1;
                npc.rarity = 4;
            }
            else {
                npc.width = 25;
                npc.height = 40;
                npc.aiStyle = 7;
                npc.rarity = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) {
            if (isUnconscious) {
                Rectangle npcRect = npc.getRect();
                npcRect.X -= (int)Main.screenPosition.X;
                npcRect.Y -= (int)Main.screenPosition.Y;
                spriteBatch.Draw(UnconsciousTexture, npcRect, drawColor);
                return false;
            }
            return base.PreDraw(spriteBatch, drawColor);
        }
        #endregion

        #region Chat
        public override string GetChat() {
            if (isUnconscious) {
                isUnconscious = false;
                npc.position.Y -= 22;
                return "Huh? Oh. Thanks for helping me up. You know, my friends up in the village really hate your guts. If you could help me get back up there, maybe I could put in a good word for you.";
            }
            else {
                return "Test dialogue moment";
            }
        }
        #endregion
    }
}
