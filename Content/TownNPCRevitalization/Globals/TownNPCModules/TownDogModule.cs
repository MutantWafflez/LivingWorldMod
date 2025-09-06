using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;

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

        foreach (Player player in Main.ActivePlayers) {
            if (!DogFetchAIState.PlayerIsValidToPlayFetchWith(player, NPC)) {
                continue;
            }

            fetchPlayer = player;
        }

        if (fetchPlayer is null) {
            return;
        }

        NPC.GetGlobalNPC<TownNPCPathfinderModule>().CancelPathfind();
        TownNPCStateModule.RefreshToState<DogFetchAIState>(NPC);
    }
}