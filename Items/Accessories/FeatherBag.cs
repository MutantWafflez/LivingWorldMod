using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Waist)]
    public class FeatherBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Unleashes a flurry of feathers when jumping or flapping your wings"
                + "\nFeathers deal additional damage based on the strength of your wings");
        }
        public override void SetDefaults()
        {
            item.width = 52;
            item.height = 36;
            item.rare = ItemRarityID.Blue;
            item.accessory = true;
            item.value = Item.buyPrice(gold: 15);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<LWMPlayer>().featherBag = true;
        }
    }
}
