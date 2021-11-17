using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.VanillaOverrides;
using LivingWorldMod.Custom.Classes;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// System that handles some Waystone functionality, in tandem with the Waystone tile entity
    /// and tiles.
    /// </summary>
    public class WaystoneSystem : ModSystem {
        public List<WaystoneInfo> waystoneData;

        public override void Load() {
            // Initialize Waystone data list
            waystoneData = new List<WaystoneInfo>();

            // Add Waystone layer to map
            Main.MapIcons.AddLayer(new WaystoneMapLayer());
        }

        public override void SaveWorldData(TagCompound tag) {
            tag["waystoneData"] = waystoneData;
        }

        public override void LoadWorldData(TagCompound tag) {
            waystoneData = tag.GetList<WaystoneInfo>("waystoneData").ToList();
        }
    }
}