using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Core;
using Hook = LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks.IUpdateSleep;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;

/// <summary>
///     This hook is ran every tick that this NPC is in the proper state to be "asleep." See <see cref="BeAtHomeAIState" /> and <see cref="PassedOutAIState" />. Called in all net-modes (singleplayer,
///     client, and server).
/// </summary>
// This implementation of a custom tML hook is based off Mirsario's in Terraria Overhaul: https://github.com/Mirsario/TerrariaOverhaul
public interface IUpdateSleep {
    public static readonly GlobalHookList<GlobalNPC> Hook = NPCLoader.AddModHook(GlobalHookList<GlobalNPC>.Create(i => ((Hook)i).UpdateSleep));

    public static void Invoke(NPC npc, Vector2? drawOffset, uint? frameOverride, bool passedOut) {
        foreach (Hook g in Hook.Enumerate(npc)) {
            g.UpdateSleep(npc, drawOffset, frameOverride, passedOut);
        }
    }

    /// <inheritdoc cref="IUpdateSleep" />
    void UpdateSleep(NPC npc, Vector2? drawOffset, uint? frameOverride, bool passedOut);
}