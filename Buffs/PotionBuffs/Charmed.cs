using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Buffs.PotionBuffs
{
    public class Charmed : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Charmed");
            Description.SetDefault("The Harpies think higher of you!");
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.loveStruck = true;
        }
    }
}