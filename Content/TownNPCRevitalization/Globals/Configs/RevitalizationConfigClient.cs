using System.Collections.Generic;
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

    public override ConfigScope Mode => ConfigScope.ClientSide;
}