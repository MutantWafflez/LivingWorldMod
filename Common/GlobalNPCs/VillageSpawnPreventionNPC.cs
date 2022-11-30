using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Custom.Utilities;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalNPCs {
    /// <summary>
    /// Simple GlobalNPC that prevents spawning if the player is within a village.
    /// </summary>
    public class VillageSpawnPreventionNPC : GlobalNPC {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
            if (TileEntityUtils.GetAllEntityOfType<VillageShrineEntity>().Any(shrine => shrine.villageZone.ContainsPoint(spawnInfo.Player.Center))) {
                foreach ((int key, float value) in pool) {
                    pool[key] = 0;
                }
            }
        }
    }
}