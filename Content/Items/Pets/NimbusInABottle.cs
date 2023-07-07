using LivingWorldMod.Content.StatusEffects.Buffs.Pets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Pets {
    //Thanks Trivaxy for the code! :-)
    public class NimbusInABottle : BaseItem {
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults() {
            Item.damage = 0;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.width = 20;
            Item.height = 26;
            Item.UseSound = SoundID.Item44;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Blue;
            Item.noMelee = true;
            Item.value = Item.sellPrice(silver: 60);
            Item.buffType = ModContent.BuffType<NimbusPetBuff>();
        }

        public override bool? UseItem(Player player) {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
                player.AddBuff(Item.buffType, 20000);
                return true;
            }

            return base.UseItem(player);
        }
    }
}