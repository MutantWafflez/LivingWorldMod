using Terraria.ModLoader.Core;
using Hook = LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks.IUpdateTownNPCSmallTalk;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;

/// <summary>
///     This hook is ran every tick that a given NPC is engaging in "small talk" with another NPC, with the flavor text bubbles occurring over their sprite. Not called on the server, since
///     NPCs cannot engage in "small talk" on the server.
/// </summary>
// This implementation of a custom tML hook is based off Mirsario's in Terraria Overhaul: https://github.com/Mirsario/TerrariaOverhaul
public interface IUpdateTownNPCSmallTalk {
    public static readonly GlobalHookList<GlobalNPC> Hook = NPCLoader.AddModHook(GlobalHookList<GlobalNPC>.Create(i => ((Hook)i).UpdateTownNPCSmallTalk));

    public static void Invoke(NPC npc, int remainingTicks) {
        foreach (Hook g in Hook.Enumerate(npc)) {
            g.UpdateTownNPCSmallTalk(npc, remainingTicks);
        }
    }

    /// <inheritdoc cref="UpdateTownNPCSmallTalk" />
    void UpdateTownNPCSmallTalk(NPC npc, int remainingTicks);
}