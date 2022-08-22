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

        [Label("Pyramid Generation Debug")]
        [Tooltip("Requires Debug Mode to be enabled to function; upon entering any world, instantly enter the Revamped Pyramid Subworld.")]
        [DefaultValue(false)]
        public bool pyramidDebug;

        public override ConfigScope Mode => ConfigScope.ServerSide;
    }
}