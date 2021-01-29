using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.NPCs.Villagers.Quest
{
    public abstract class QuestVillager : ModNPC
    {
        //What type of villager this Quest Villager represents. Villager Count by default.
        public virtual VillagerType VillagerType => VillagerType.VillagerTypeCount;

        /// <summary>
        /// A list of all of the possible chats that can be said when interacting with the player (and not pressing any buttons)
        /// </summary>
        public virtual WeightedRandom<string> InteractionDialogue => new WeightedRandom<string>();

        /// <summary>
        /// What text is said when the given quest is completed.
        /// </summary>
        public virtual string QuestCompletetionDialogue => "Quest complete default text. Uhhh... report this, probably.";

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

        public override void OnChatButtonClicked(bool firstButton, ref bool shop) {
            if (firstButton && LWMWorld.GetActiveQuest(VillagerType).ActivationCondition(Main.LocalPlayer, npc)) {
                Main.npcChatText = QuestCompletetionDialogue;
                LWMWorld.RefreshVillageQuest(VillagerType);
                LWMWorld.SetReputation(VillagerType, 15);
            }
            else if (firstButton) {
                Main.npcChatText = LWMWorld.activeQuests[(int)VillagerType].questText;
            }
        }

        #endregion Chat
    }
}