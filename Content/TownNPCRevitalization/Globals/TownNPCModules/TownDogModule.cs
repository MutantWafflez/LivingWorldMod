using System.IO;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

public class TownDogModule : TownNPCModule {
    private static TrackedProjectileReference _projReference;

    public Player fetchPlayer;
    public Projectile fetchProj;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => lateInstantiation && entity.type == NPCID.TownDog;

    public override void UpdateModule() {
        CheckForFetch();
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) {
        binaryWriter.Write(fetchPlayer?.whoAmI ?? -1);

        bool hasFetchProj = fetchProj is not null;
        bitWriter.WriteBit(hasFetchProj);

        if (!hasFetchProj) {
            return;
        }

        _projReference.Set(fetchProj);
        _projReference.Write(binaryWriter);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) {
        int fetchPlayerWhoAmI = binaryReader.ReadInt32();
        fetchPlayer = fetchPlayerWhoAmI < 0 ? null : Main.player[fetchPlayerWhoAmI];

        bool hasProjectileInfo = bitReader.ReadBit();
        if (!hasProjectileInfo) {
            return;
        }

        _projReference.TryReading(binaryReader);
        if (!_projReference.IsTrackingSomething) {
            fetchPlayer = null;
            fetchProj = null;

            return;
        }

        fetchProj = Main.projectile[_projReference.ProjectileLocalIndex];
    }

    private void CheckForFetch() {
        if (Main.netMode == NetmodeID.MultiplayerClient || NPC.ai[0] == TownNPCAIState.GetStateInteger<DogFetchAIState>()) {
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