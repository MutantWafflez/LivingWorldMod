using System.Linq;
using Terraria;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Utilities;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Commands.DebugCommands {
    /// <summary>
    /// Debug Command that will disable all Waystones in this world. Be careful with this.
    /// </summary>
    public class DisableWaystonesCommand : DebugCommand {
        public override string Command => "disableway";

        public override string Usage => "/disableway";

        public override string Description => "Deactivates all Waystones in this world.";

        public override CommandType Type => CommandType.World;

        public override void Action(CommandCaller caller, string input, string[] args) {
            foreach (WaystoneEntity entity in TileEntityUtils.GetAllEntityOfType<WaystoneEntity>()) {
                entity.isActivated = false;
                if (Main.netMode == NetmodeID.Server) {
                    NetMessage.SendData(MessageID.TileEntitySharing, number: entity.ID, number2: entity.Position.X, number3: entity.Position.Y);
                }
            }
        }
    }
}