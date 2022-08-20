using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Commands.DebugCommands {
    /// <summary>
    /// Command that triggers instantaneous death in the caller.
    /// </summary>
    public class DieCommand : DebugCommand {
        public override string Command => "die";

        public override string Usage => "/die";

        public override string Description => "Instantly kills the user.";

        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args) {
            caller.Player.KillMe(PlayerDeathReason.ByCustomReason("Death Disease"), caller.Player.statLife, 0);
        }
    }
}