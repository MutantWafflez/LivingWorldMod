namespace LivingWorldMod.Globals.BaseTypes.Commands;

/// <summary>
/// Command that is loaded only in Debug mode.
/// </summary>
public abstract class DebugCommand : ModCommand {
    public override bool IsLoadingEnabled(Mod mod) => LWM.IsDebug;
}