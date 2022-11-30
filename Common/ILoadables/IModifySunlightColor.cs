using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.ILoadables {
    /// <summary>
    /// Interface that, once inherited, will have some kind of effect on the sunlight color based on the
    /// specified checks.
    /// </summary>
    public interface IModifySunlightColor : ILoadable {
        /// <summary>
        /// Whether or not this current effect should be active.
        /// </summary>
        public bool SunlightEffectActive {
            get;
        }

        /// <summary>
        /// Method that applies this object's sunlight on the screen. Remember this scale can be different if other effects
        /// are simultaneously active.
        /// </summary>
        public void SunlightEffect(ref Color tileColor, ref Color backgroundColor);

        [NoJIT]
        void ILoadable.Load(Mod mod) { }

        [NoJIT]
        void ILoadable.Unload() { }
    }
}