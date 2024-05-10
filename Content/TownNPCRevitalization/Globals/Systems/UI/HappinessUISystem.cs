using LivingWorldMod.Content.TownNPCRevitalization.UI.Happiness;
using LivingWorldMod.Globals.Systems.BaseSystems;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;

public class HappinessUISystem : UISystem<HappinessUIState> {
    public override string InternalInterfaceName => "Town NPC Happiness";

    public void OpenHappinessState(NPC npc) {
        correspondingUIState.SetStateToNPC(npc);
        correspondingInterface.SetState(correspondingUIState);
    }
}