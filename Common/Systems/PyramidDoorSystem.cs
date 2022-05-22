using System.Collections.Generic;
using LivingWorldMod.Content.MiscEntities;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {
    [Autoload(Side = ModSide.Client)]
    public class PyramidDoorSystem : ModSystem {
        private List<PyramidDoorOpenEntity> _openingEntities;

        public override void OnWorldLoad() {
            _openingEntities = new List<PyramidDoorOpenEntity>();
        }

        public override void PostUpdateEverything() {
            _openingEntities.RemoveAll(entity => entity.isFinished);
            _openingEntities.ForEach(entity => entity.Update());
        }

        /// <summary>
        /// Adds a new activation entity at the specified location. Remember the center is in TILE coordinates (not world).
        /// </summary>
        /// <param name="entityPosition"> The top left of the entity. </param>
        public void AddNewActivationEntity(Point16 entityPosition) {
            _openingEntities.Add(new PyramidDoorOpenEntity(entityPosition));
        }
    }
}