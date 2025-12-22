using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Players;
using LivingWorldMod.Utilities;
using Terraria.Chat;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Commands;

// TODO: Fix actual underlying issue necessitating this command
public class KillDupeNPCsCommand : ModCommand {
    public override string Command => "killdupe";

    public override string Usage => "/killdupe";

    public override string Description => "Kills all Town NPCs in the world which are 'duplicates', i.e. all additional Town NPCs which have no name and are just 'Guide' or 'Merchant' and so on.";

    public override CommandType Type => CommandType.World;

    public override void Action(CommandCaller caller, string input, string[] args) {
        LocalizedText killMessage = "Debug.KillDupeNPCs".Localized();
        if (Main.netMode == NetmodeID.Server) {
            ChatHelper.BroadcastChatMessage(killMessage.ToNetworkText(), LWMUtils.RedTownNPCDeathTextColor);
        }
        else {
            Main.NewText(killMessage, LWMUtils.RedTownNPCDeathTextColor);
        }

        Dictionary<int, List<int>> allTownNPCs = CheckDupeTownNPCsPlayer.GetAllTownNPCs();
        foreach ((int _, List<int> npcWhoAmIs) in allTownNPCs) {
            if (npcWhoAmIs.Count <= 1) {
                continue;
            }

            foreach (int whoAmI in npcWhoAmIs) {
                NPC npc = Main.npc[whoAmI];
                if (npc.HasGivenName) {
                    continue;
                }

                npc.StrikeInstantKill();
            }
        }
    }
}