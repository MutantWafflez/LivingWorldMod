using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.NPCs.Villagers.Quest
{
    public abstract class QuestVillager : ModNPC
    {
        //What type of villager this Quest Villager represents. Villager Count by default.
        public virtual VillagerType VillagerType => VillagerType.VillagerTypeCount;

        #region Defaults

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

        #endregion Defaults

        #region Spawning

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.player.ZoneOverworldHeight)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].GetType() == GetType())
                    {
                        return 0f;
                    }
                }

                if (LWMWorld.GetReputation(VillagerType) <= 0f)
                {
                    return 0.33f;
                }
            }
            return 0f;
        }

        #endregion Spawning

        #region Chat

        public override bool CanChat() => true;

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "Quest";
        }

        #endregion Chat
    }
}