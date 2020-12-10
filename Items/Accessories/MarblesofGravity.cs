using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Accessories
{
    public class MarblesofGravity : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Marbles of Gravity");
            Tooltip.SetDefault("You no longer lose gravity in space");
        }
        public override void SetDefaults()
        {
            item.width = 36;
            item.height = 36;
            item.rare = ItemRarityID.Blue;
            item.accessory = true;
            item.value = Item.buyPrice(gold: 25);
        }

        public override void UpdateEquip(Player player)
        {
            if (player.ZoneSkyHeight)
            {
                player.gravity = 0.4f;
            }
            // ... Easy as that
        }
    }
}
