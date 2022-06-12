using System.Collections.Generic;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Common.ModTypes {
    /// <summary>
    /// ModType that is used for WorldGen features. Provides the <see cref="Generate"/> method for
    /// this feature's generation as well as other methods for additional functionality. Exists to
    /// alleviate the size of <see cref="WorldCreationSystem"/>.
    /// </summary>
    public abstract class WorldGenFeature : ModType {
        /// <summary>
        /// The internal name of this world feature, used when creating a new <seealso
        /// cref="Terraria.GameContent.Generation.PassLegacy"/> in the world generation list. Must
        /// be defined.
        /// </summary>
        public abstract string InternalGenerationName {
            get;
        }

        /// <summary>
        /// The string name of the (probably) Vanilla generation pass that this feature will use as
        /// a reference point for placement. Must be defined.
        /// </summary>
        public abstract string InsertionPassNameForFeature {
            get;
        }

        /// <summary>
        /// Whether or not this feature should be placed before (true) or after (false) the defined
        /// <see cref="InsertionPassNameForFeature"/> value. Defaults to true.
        /// </summary>
        public virtual bool PlaceBeforeInsertionPoint => true;

        /// <summary>
        /// The size of the world that is currently being generated.
        /// </summary>
        public WorldSize CurrentWorldSize => WorldGenUtils.CurrentWorldSize;

        /// <summary>
        /// Allows for modification of the list directly. Do not use this to add new passes, only
        /// use this if you want to remove/move other established passes.
        /// </summary>
        /// <param name="tasks"> Lists of tasks when this method is run. </param>
        public virtual void ModifyTaskList(List<GenPass> tasks) { }

        /// <summary>
        /// Method that should generate this feature.
        /// </summary>
        /// <param name="progress">
        /// The <see cref="GenerationProgress"/> instance given for this feature.
        /// </param>
        /// <param name="gameConfig"> The current game config. </param>
        public virtual void Generate(GenerationProgress progress, GameConfiguration gameConfig) { }

        /// <summary>
        /// Method ran after all WorldGen tasks have completed.
        /// </summary>
        public virtual void PostWorldGenAction() { }

        public sealed override void SetupContent() => SetStaticDefaults();

        protected sealed override void Register() {
            ModTypeLookup<WorldGenFeature>.Register(this);
        }
    }
}