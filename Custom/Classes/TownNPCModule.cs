using LivingWorldMod.Common.GlobalNPCs;
using Terraria;

namespace LivingWorldMod.Custom.Classes {
    public abstract class TownNPCModule {
        protected TownAIGlobalNPC GlobalNPC => npc.GetGlobalNPC<TownAIGlobalNPC>();

        protected readonly NPC npc;

        protected TownNPCModule(NPC npc) {
            this.npc = npc;
        }

        public abstract void Update();
    }
}