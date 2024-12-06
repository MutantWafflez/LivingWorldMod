using LivingWorldMod.Content.TownNPCRevitalization.UI.Happiness;
using LivingWorldMod.Globals.BaseTypes.Systems;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;

[Autoload(false)]
public class HappinessUISystem : UISystem<HappinessUISystem, HappinessUIState> {
    public override string InternalInterfaceName => "Town NPC Happiness";

    public override void PostUpdateEverything() {
        int talkNPC = Main.LocalPlayer.talkNPC;
        if (correspondingInterface.CurrentState == null || (talkNPC != -1 && (Main.npc[talkNPC]?.type ?? -1) == correspondingUIState.NPCBeingTalkedTo.type)) {
            return;
        }

        correspondingInterface.SetState(null);
        correspondingUIState.ClearState();
    }

    public void OpenHappinessState(NPC npc) {
        correspondingUIState.SetStateToNPC(npc);
        correspondingInterface.SetState(correspondingUIState);
    }
}