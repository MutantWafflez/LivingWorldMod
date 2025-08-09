using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Items;
using LivingWorldMod.Utilities;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

public class TownDogModule : TownNPCModule {
    private const int MaxTileDistanceToPlayFetch = LWMUtils.TilePixelsSideLength * 40;

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
            if (player.HeldItem.type != ModContent.ItemType<FetchingStick>()
                || NPC.Distance(player.Center) > MaxTileDistanceToPlayFetch * MaxTileDistanceToPlayFetch
                || !Collision.CanHitLine(NPC.Center, 2, 2, player.Center, 2, 2)
            ) {
                continue;
            }

            fetchPlayer = player;
        }

        if (fetchPlayer is null) {
            return;
        }

        TownNPCStateModule.RefreshToState<DogFetchAIState>(NPC);
    }
}