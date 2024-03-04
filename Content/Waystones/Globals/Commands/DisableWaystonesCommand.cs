using LivingWorldMod.Content.Waystones.Tiles;
using LivingWorldMod.Globals.BaseTypes.Commands;
using LivingWorldMod.Utilities;

namespace LivingWorldMod.Content.Waystones.Globals.Commands;

/// <summary>
/// Debug Command that will disable all Waystones in this world. Be careful with this.
/// </summary>
public class DisableWaystonesCommand : DebugCommand {
    public override string Command => "disableway";

    public override string Usage => "/disableway";

    public override string Description => "Deactivates all Waystones in this world.";

    public override CommandType Type => CommandType.World;

    public override void Action(CommandCaller caller, string input, string[] args) {
        foreach (WaystoneEntity entity in LWMUtils.GetAllEntityOfType<WaystoneEntity>()) {
            entity.isActivated = false;
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.TileEntitySharing, number: entity.ID, number2: entity.Position.X, number3: entity.Position.Y);
            }
        }
    }
}