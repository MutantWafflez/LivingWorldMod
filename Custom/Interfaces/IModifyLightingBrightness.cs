using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Interfaces {
    public interface IModifyLightingBrightness : ILoadable {
        /// <summary>
        /// Whether or not this current effect should be active.
        /// </summary>
        public bool LightingEffectActive {
            get;
        }

        /// <summary>
        /// Method that applies this object's lighting effect on the screen. Remember this scale can be different if other effects
        /// are simultaneously active.
        /// </summary>
        /// <param name="scale"> The current scale of the lighting. </param>
        public void LightingEffect(ref float scale);

        void ILoadable.Load(Mod mod) { }

        void ILoadable.Unload() { }
    }
}