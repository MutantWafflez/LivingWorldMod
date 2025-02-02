using Terraria.ModLoader.Core;
using Hook = LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks.IOnTownNPCAttack;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;

/// <summary>
///     This hook will be triggered on the very first tick that a Town NPC attacks something. Called on the server and in singleplayer, but not on MP Clients.
/// </summary>
// This implementation of a custom tML hook is based off Mirsario's in Terraria Overhaul: https://github.com/Mirsario/TerrariaOverhaul
public interface IOnTownNPCAttack {
    public static readonly GlobalHookList<GlobalNPC> Hook = NPCLoader.AddModHook(GlobalHookList<GlobalNPC>.Create(i => ((Hook)i).OnTownNPCAttack));

    public static void Invoke(NPC npc) {
        foreach (Hook g in Hook.Enumerate(npc)) {
            g.OnTownNPCAttack(npc);
        }
    }

    /// <inheritdoc cref="IOnTownNPCAttack"/>
    void OnTownNPCAttack(NPC npc);
}