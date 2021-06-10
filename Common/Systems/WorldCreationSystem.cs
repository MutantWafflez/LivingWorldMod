using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.Systems {

    /// <summary>
    /// System that handles the INITIAL world generation steps. This system does NOT handle world
    /// events that occur AFTER the world is created.
    /// </summary>
    public class WorldCreationSystem : ModSystem {

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
        }
    }
}