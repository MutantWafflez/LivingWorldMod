using Terraria.ModLoader;

namespace LivingWorldMod.Content.Walls.DebugWalls {

    /// <summary>
    /// Wall that is only loaded when in Debug mode.
    /// </summary>
    public abstract class DebugWall : ModWall {

        public override bool IsLoadingEnabled(Mod mod) => LivingWorldMod.IsDebug;
    }
}