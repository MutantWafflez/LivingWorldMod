using System.Linq;
using LivingWorldMod.Custom.Interfaces;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// ModSystem that handles any classes that implemented the <seealso cref="IPlayerEnteredWorld"/> interface, and calls their
    /// respective method when necessary.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class PlayerEnteredWorldSystem : ModSystem {
        public override void OnWorldLoad() {
            ModContent.GetContent<IPlayerEnteredWorld>().ToList().ForEach(thing => thing.OnPlayerEnterWorld());
        }
    }
}