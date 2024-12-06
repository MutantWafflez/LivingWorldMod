using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Configs;

[Autoload(false)]
public class TownNPCConfig : ModConfig {
    [Range(16, 512)]
    [DefaultValue(128)]
    public int pathfinderSize;

    public override ConfigScope Mode => ConfigScope.ServerSide;
}