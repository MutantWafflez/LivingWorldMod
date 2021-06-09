using Terraria.ModLoader;

namespace LivingWorldMod {

    public class LivingWorldMod : Mod {

        public static LivingWorldMod Instance {
            get;
            internal set;
        }

        public LivingWorldMod() {
            Instance = this;
        }

        public override void Load() {
        }

        public override void Unload() {
            Instance = null;
        }
    }
}