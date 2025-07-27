using LivingWorldMod.Content.TownNPCRevitalization.UI.Happiness;
using LivingWorldMod.Globals.BaseTypes.Systems;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;

public class HappinessUISystem : UISystem<HappinessUISystem, HappinessUIState> {
    public override string InternalInterfaceName => "Town NPC Happiness";

    public override void PostUpdateEverything() {
        int talkNPC = Main.LocalPlayer.talkNPC;
        if (!UIIsActive || (talkNPC != -1 && (Main.npc[talkNPC]?.type ?? -1) == UIState.NPCBeingTalkedTo.type)) {
            return;
        }

        UIState.ClearState();

        CloseUIState();
    }

    public void OpenHappinessState(NPC npc) {
        OpenUIState();
        
        UIState.SetStateToNPC(npc);
    }
}