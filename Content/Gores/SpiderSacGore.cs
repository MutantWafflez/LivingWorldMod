using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Gores {

    public class SpiderSacGore : ModGore {

        public override void OnSpawn(Gore gore) {
            gore.behindTiles = false;
            gore.alpha = 0;
            gore.timeLeft = 5 * 60;
            gore.sticky = true;
        }

        public override bool Update(Gore gore) {
            if (--gore.timeLeft <= 0)
                gore.active = false;
            else if (gore.timeLeft <= 60)
                gore.alpha += 5;
            return true;
        }
    }
}