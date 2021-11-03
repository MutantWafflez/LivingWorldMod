using Terraria.ModLoader;

namespace LivingWorldMod.Common.Commands.DebugCommands {

    /// <summary>
    /// Command that is loaded only in Debug mode.
    /// </summary>
    public abstract class DebugCommand : ModCommand {

        public override bool IsLoadingEnabled(Mod mod) => LivingWorldMod.IsDebug;
    }
}