using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Accessories
{
    //[AutoloadEquip(EquipType.Waist)]
    public class FeatherBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Unleash a flurry of feathers when in the air");
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
            LWMPlayer.featherBag = true;
        }
    }
}
