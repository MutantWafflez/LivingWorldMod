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
    ///     The zoom percentage required to see the actual words of Town NPC small talk instead of the emote bubbles. Defaults to 1.67, or 167%.
    /// </summary>
    [Range(1f, 4f)]
    [DefaultValue(1.67f)]
    public float minimumZoomToSeeSmallTalk;

    /// <summary>
    ///     The size of the drawn text used for Town NPC small talk.
    /// </summary>
    [Range(0.1f, 2f)]
    [DefaultValue(0.55f)]
    public float smallTalkTextSize;

    public override ConfigScope Mode => ConfigScope.ClientSide;
}