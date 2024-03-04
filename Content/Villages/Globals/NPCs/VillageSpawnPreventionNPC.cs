using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;
using LivingWorldMod.Utilities;

namespace LivingWorldMod.Content.Villages.Globals.NPCs;

/// <summary>
/// Simple GlobalNPC that prevents spawning if the player is within a village.
/// </summary>
public class VillageSpawnPreventionNPC : GlobalNPC {
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
        if (LWMUtils.GetAllEntityOfType<VillageShrineEntity>().Any(shrine => shrine.villageZone.ContainsPoint(spawnInfo.Player.Center))) {
            foreach ((int key, float value) in pool) {
                pool[key] = 0;
            }
        }
    }
}