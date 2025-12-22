using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Players;

// TODO: Fix actual underlying issue necessitating this Player class 
public class CheckDupeTownNPCsPlayer : ModPlayer {
    public static Dictionary<int, List<int>> GetAllTownNPCs() {
        Dictionary<int, List<int>> allTownNPCs = [];
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!TownGlobalNPC.IsAnyValidTownNPC(npc, true)) {
                continue;
            }

            if (!allTownNPCs.TryGetValue(npc.type, out List<int> whoAmIs)) {
                allTownNPCs[npc.type] = [npc.whoAmI];

                continue;
            }

            whoAmIs.Add(npc.whoAmI);
        }

        return allTownNPCs;
    }

    public override void OnEnterWorld() {
        Dictionary<int, List<int>> allTownNPCs = GetAllTownNPCs();

        if (!allTownNPCs.Any(pair => pair.Value.Count > 1)) {
            return;
        }

        Main.NewText("Debug.DuplicateNPCsWarning".Localized(), LWMUtils.YellowErrorTextColor);
    }
}