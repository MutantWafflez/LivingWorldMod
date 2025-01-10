using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.DataStructures.Classes;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;

public sealed class TownNPCStateModule : TownNPCModule {
    /// <summary>
    ///     How many activities are remembered by this NPC for the
    ///     purpose of preventing activity repetition.
    /// </summary>
    private const int LastActivityMemoryLimit = 5;

    private static IReadOnlyDictionary<int, TownNPCAIState> _stateDict;
    private static IReadOnlyList<TownNPCActivity> _allActivities;

    private ForgetfulArray<TownNPCActivity> _lastActivities = new(LastActivityMemoryLimit);

    public static void RefreshToState<T>(NPC npc) where T : TownNPCAIState => RefreshToState(npc, TownNPCAIState.GetStateInteger<T>());

    public static void RefreshToState(NPC npc, int stateValue) {
        npc.ai[0] = stateValue;
        npc.ai[1] = npc.ai[2] = npc.ai[3] = 0;
        npc.netUpdate = true;
    }

    public override void SetStaticDefaults() {
        List<TownNPCAIState> states = ModContent.GetContent<TownNPCAIState>().ToList();

        if (states.Count != states.DistinctBy(state => state.ReservedStateInteger).Count()) {
            throw new Exception("Multiple TownNPCAIState instances with the same ReservedStateInteger");
        }

        _stateDict = states.ToDictionary(state => state.ReservedStateInteger);
        //_allActivities = states.OfType<TownNPCActivity>().ToList();
    }

    public override void UpdateModule() {
        if (_stateDict.TryGetValue((int)NPC.ai[0], out TownNPCAIState state)) {
            state.DoState(NPC);

            /*
            // New activities can only be selected when the npc is in the default state
            if (state is DefaultAIState && Main.rand.Next(_allActivities.SkipWhile(_lastActivities.Contains).ToList()) is { } activity && activity.CanDoActivity(this, npc)) {
                _lastActivities.Add(activity);
                activity.InitializeActivity(npc);

                return;
            }*/
        }
        else {
            RefreshToState<DefaultAIState>(NPC);
        }
    }
}