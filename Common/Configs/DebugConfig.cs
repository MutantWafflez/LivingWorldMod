using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LivingWorldMod.Common.Configs {
    /// <summary>
    /// Config that handles debug related matters in the mod.
    /// </summary>
    public class DebugConfig : ModConfig {
        [Label("Force Debug Mode")]
        [Tooltip("Forces the mod to enter Debug mode, regardless of if the mod is being built from Visual Studio.\nONLY enable this if you know what you're doing.")]
        [DefaultValue(false)]
        [ReloadRequired]
        public bool forceDebugMode;

        public override ConfigScope Mode => ConfigScope.ServerSide;
    }
}