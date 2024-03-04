using System.Collections.Generic;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Globals.Systems.BaseSystems;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Waystones.Globals.Systems;

/// <summary>
/// ModSystem that helps with client-side Waystone functionality.
/// </summary>
[Autoload(Side = ModSide.Client)]
public class WaystoneSystem : BaseModSystem<WaystoneSystem> {
    private List<WaystoneActivationEntity> _activationEntities;

    public override void OnWorldLoad() {
        _activationEntities = new List<WaystoneActivationEntity>();
    }

    public override void PostUpdateEverything() {
        _activationEntities.RemoveAll(entity => entity.isFinished);
        _activationEntities.ForEach(entity => entity.Update());
    }

    /// <summary>
    /// Adds a new activation entity at the specified location. Remember the center is in WORLD coordinates (not tiles).
    /// </summary>
    /// <param name="entityCenter"> The center of the entity. </param>
    /// <param name="waystoneColor"> The color of the Waystone in question. </param>
    public void AddNewActivationEntity(Vector2 entityCenter, Color waystoneColor) {
        _activationEntities.Add(new WaystoneActivationEntity(entityCenter, waystoneColor));
    }
}