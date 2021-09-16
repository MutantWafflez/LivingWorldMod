using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LivingWorldMod.Content.TileEntities;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {

    /// <summary>
    /// ModSystem that handles Tile Entities and their functionality beyond what is already defined
    /// in the specified tile entity classes.
    /// </summary>
    public class TileEntitySystem : ModSystem {
        public static List<BaseTileEntity> tileEntities;

        public override void Load() {
            tileEntities = new List<BaseTileEntity>();

            foreach (Type tileEntityType in Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(BaseTileEntity)) && !type.IsAbstract)) {
                tileEntities.Add((BaseTileEntity)Activator.CreateInstance(tileEntityType));
            }
        }

        public override void Unload() {
            tileEntities = null;
        }
    }
}