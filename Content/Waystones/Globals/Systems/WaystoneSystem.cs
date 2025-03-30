using System.Collections.Generic;
using System.IO;
using LivingWorldMod.Content.Waystones.DataStructures.Classes;
using LivingWorldMod.Content.Waystones.Tiles;
using LivingWorldMod.Globals.BaseTypes.Systems;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Waystones.Globals.Systems;

/// <summary>
///     ModSystem that handles various miscellaneous waystone functionality. Namely, it handles updating <see cref="WaystoneActivationEntity" /> entities and syncing waystones across the server.
/// </summary>
public class WaystoneSystem : BaseModSystem<WaystoneSystem> {
    private List<WaystoneActivationEntity> _activationEntities;

    public override void OnWorldLoad() {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        _activationEntities = [];
    }

    public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber) {
        // This message ID is sent as a part of the client joining process, where spawn tile information is usually synced.
        // The name is slightly misleading however, as this is also where pylons are synced, so I feel it is only natural
        // for waystones to also be synced properly here.
        if (Main.netMode != NetmodeID.Server || messageType is not MessageID.SpawnTileData) {
            return false;
        }

        foreach (WaystoneEntity entity in LWMUtils.GetAllEntityOfType<WaystoneEntity>()) {
            NetMessage.SendData(MessageID.TileEntitySharing, playerNumber, number: entity.ID, number2: entity.Position.X, number3: entity.Position.Y);
        }

        return false;
    }

    public override void PostUpdateEverything() {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        _activationEntities.RemoveAll(entity => entity.isFinished);
        _activationEntities.ForEach(entity => entity.Update());
    }

    /// <summary>
    ///     Adds a new activation entity at the specified location. Remember the center is in WORLD coordinates (not tiles).
    /// </summary>
    /// <param name="entityCenter"> The center of the entity. </param>
    /// <param name="waystoneColor"> The color of the Waystone in question. </param>
    public void AddNewActivationEntity(Vector2 entityCenter, Color waystoneColor) {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        _activationEntities.Add(new WaystoneActivationEntity(entityCenter, waystoneColor));
    }
}