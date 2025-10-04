using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Configs;

/// <summary>
///     Mod Config for the Town NPC Revitalization, relating strictly to client-side configuration.
/// </summary>
public class RevitalizationConfigClient : ModConfig {
    /// <summary>
    ///     Set of NPCs that will have their "Draw Overhauls" disabled, i.e. they will not have the "blink" or "talk" animations overlayed.
    /// </summary>
    public HashSet<NPCDefinition> disabledDrawOverhauls = [];

    /// <summary>
    ///     The zoom percentage required to see the actual words of Town NPC small talk instead of the emote bubbles. Defaults to 1.75, or 175%.
    /// </summary>
    [Range(1f, 4f)]
    [DefaultValue(1.75f)]
    public float minimumZoomToSeeSmallTalk;

    public override ConfigScope Mode => ConfigScope.ClientSide;
}