using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Items;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

public class TownDogModule : TownNPCModule {
    public Player fetchPlayer;
    public Projectile fetchProj;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => lateInstantiation && entity.type == NPCID.TownDog;

    public override void UpdateModule() {
        CheckForFetch();
    }

    private void CheckForFetch() {
        if (NPC.ai[0] == TownNPCAIState.GetStateInteger<DogFetchAIState>()) {
            return;
        }

        fetchPlayer = null;
        fetchProj = null;

        if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<FetchingStick>()) {
            return;
        }

        TownNPCStateModule.RefreshToState<DogFetchAIState>(NPC);

        fetchPlayer = Main.LocalPlayer;
    }
}