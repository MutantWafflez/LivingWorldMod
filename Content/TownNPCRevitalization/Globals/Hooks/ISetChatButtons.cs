using Terraria.ModLoader.Core;
using Hook = LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks.ISetChatButtons;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;

/// <summary>
///     A <see cref="GlobalNPC" /> version of the <see cref="ModNPC.SetChatButtons" /> hook.
/// </summary>
// This implementation of a custom tML hook is based off Mirsario's in Terraria Overhaul: https://github.com/Mirsario/TerrariaOverhaul
public interface ISetChatButtons {
    public static readonly GlobalHookList<GlobalNPC> Hook = NPCLoader.AddModHook(GlobalHookList<GlobalNPC>.Create(i => ((Hook)i).SetChatButtons));

    public static void Invoke(ref string buttonOne, ref string buttonTwo) {
        if (Main.LocalPlayer.TalkNPC is not { } npc) {
            return;
        }

        foreach (Hook g in Hook.Enumerate(npc)) {
            g.SetChatButtons(npc, ref buttonOne, ref buttonTwo);
        }
    }

    /// <inheritdoc cref="ISetChatButtons" />
    void SetChatButtons(NPC npc, ref string buttonOne, ref string buttonTwo);
}