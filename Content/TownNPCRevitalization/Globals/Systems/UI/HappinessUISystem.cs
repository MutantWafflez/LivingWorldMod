using LivingWorldMod.Content.TownNPCRevitalization.UI.Happiness;
using LivingWorldMod.Globals.BaseTypes.Systems;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;

public class HappinessUISystem : UISystem<HappinessUISystem, HappinessUIState> {
    public override string InternalInterfaceName => "Town NPC Happiness";

    public override void PostUpdateEverything() {
        if (correspondingInterface.CurrentState == null || (Main.LocalPlayer.TalkNPC is { } npc && npc == correspondingUIState.NPCBeingTalkedTo)) {
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