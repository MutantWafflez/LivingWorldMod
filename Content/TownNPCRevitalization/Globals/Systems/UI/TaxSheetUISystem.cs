using LivingWorldMod.Content.TownNPCRevitalization.UI.TaxSheet;
using LivingWorldMod.Globals.BaseTypes.Systems;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;

public class TaxSheetUISystem : UISystem<TaxSheetUISystem, TaxSheetUIState> {
    public override string InternalInterfaceName => "Tax Collector Taxes Sheet";

    public override void PostUpdateEverything() { }

    public override void UpdateUI(GameTime gameTime) {
        base.UpdateUI(gameTime);

        if (!UIIsActive || (Main.LocalPlayer.TalkNPC is { } npc && npc == UIState.NPCBeingTalkedTo)) {
            return;
        }

        CloseUIState();
    }

    public void OpenTaxesState(NPC npc) {
        OpenUIState();

        UIState.SetStateToNPC(npc);
    }
}