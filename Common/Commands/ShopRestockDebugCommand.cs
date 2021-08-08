#if DEBUG

using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Commands {

    public class ShopRestockDebugCommand : ModCommand {
        public override string Command => "restock";

        public override string Description => "Restocks all the shops of the given villager type.";

        public override string Usage => "/restock <VillagerType>";

        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args) {
            if (args.Length == 1 && int.TryParse(args[0], out int type)) {
                if (type >= 0 && type < (int)VillagerType.TypeCount) {
                    for (int i = 0; i < Main.maxNPCs; i++) {
                        if (Main.npc[i].ModNPC is HarpyVillager villager) {
                            villager.RestockShop();
                        }
                    }
                    caller.Reply((VillagerType)type + " Villagers' shops successfully restocked!");
                }
                else {
                    throw new UsageException("Did not input correct Village Type: " + type);
                }
            }
            else {
                throw new UsageException("Did not input a Village Type.");
            }
        }
    }
}

#endif