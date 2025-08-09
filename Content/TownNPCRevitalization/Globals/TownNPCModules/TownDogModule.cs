using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Items;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

public class TownDogModule : TownNPCModule {
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => lateInstantiation && entity.type == NPCID.TownDog;

    public override void UpdateModule() {
        if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<FetchingStick>() && NPC.ai[0] != TownNPCAIState.GetStateInteger<DogFetchAIState>()) {
            TownNPCStateModule.RefreshToState<DogFetchAIState>(NPC);
        }
    }
}