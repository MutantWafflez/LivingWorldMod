using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Interfaces {
    /// <summary>
    /// Interface that has its only method called when a player enters a world for THAT player only.
    /// </summary>
    public interface IPlayerEnteredWorld : ILoadable {
        /// <summary>
        /// Called for THE player/client that just entered the world, and not for anyone else.
        /// </summary>
        public void OnPlayerEnterWorld();

        [NoJIT]
        void ILoadable.Load(Mod mod) { }

        [NoJIT]
        void ILoadable.Unload() { }
    }
}