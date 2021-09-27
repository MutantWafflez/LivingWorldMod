using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles {

    /// <summary>
    /// Simple class that is the base class for all tiles in this mod. For the time being, all it
    /// does it redirect the Tile Texture to the Assets folder, where all sprites are kept.
    /// </summary>
    public abstract class BaseTile : ModTile {

        public override string Texture => GetType().Namespace?
            .Replace($"{nameof(LivingWorldMod)}.Content", LivingWorldMod.LWMSpritePath)
            .Replace('.', '/')
            + $"/{Name}";
    }
}