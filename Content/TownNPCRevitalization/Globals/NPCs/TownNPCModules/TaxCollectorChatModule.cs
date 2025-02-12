using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.Utilities;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;

/// <summary>
///     Specific module for the <see cref="NPCID.TaxCollector" /> Town NPC that handles his specific chat interactions.
/// </summary>
public class TaxCollectorChatModule : TownNPCModule, ISetChatButtons {
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => lateInstantiation && entity.type == NPCID.TaxCollector;

    public override void OnChatButtonClicked(NPC npc, bool firstButton) {
        if (firstButton) {
            return;
        }

        // Open UI here
        TaxesUISystem.Instance.OpenTaxesState(npc);
    }

    // Add second button to tax collector chat box that opens a custom new UI
    public void SetChatButtons(NPC npc, ref string buttonOne, ref string buttonTwo) {
        buttonTwo = "NPCChatButtons.TaxCollector.TaxSheet".Localized().Value;
    }
}