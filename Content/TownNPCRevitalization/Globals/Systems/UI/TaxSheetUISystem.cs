using LivingWorldMod.Content.TownNPCRevitalization.UI.TaxSheet;
using LivingWorldMod.Globals.BaseTypes.Systems;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;

public class TaxSheetUISystem : UISystem<TaxSheetUISystem, TaxSheetUIState> {
    public override string InternalInterfaceName => "Tax Collector Taxes Sheet";

    public override void PostUpdateEverything() {
        if (correspondingInterface.CurrentState == null || (Main.LocalPlayer.TalkNPC is { } npc && npc == correspondingUIState.NPCBeingTalkedTo)) {
            return;
        }

        correspondingInterface.SetState(null);
    }

    public void OpenTaxesState(NPC npc) {
        correspondingUIState.SetStateToNPC(npc);
        correspondingInterface.SetState(correspondingUIState);
    }
}