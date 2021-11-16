using LivingWorldMod.Content.TileEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {

    /// <summary>
    /// ModSystem that handles Tile Entities and their functionality beyond what is already defined
    /// in the specified tile entity classes.
    /// </summary>
    public class TileEntitySystem : ModSystem {
        public static List<BaseTileEntity> tileEntities;

        /// <summary>
        /// Searches and returns the base tile entity of the specified type.
        /// Use this instance to place entities of said type.
        /// </summary>
        public static T GetBaseEntityInstance<T>() where T : BaseTileEntity {
            return (T)tileEntities.Find(entity => entity.GetType() == typeof(T));
        }

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