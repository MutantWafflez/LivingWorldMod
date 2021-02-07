using LivingWorldMod.Buffs.PetBuffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items.Extra
{
	public class NimbusInABottle : ModItem
	{
		public override void SetStaticDefaults()
		{
			// TODO: Localization
			DisplayName.SetDefault("Nimbus in a bottle");
			Tooltip.SetDefault("Summons a pet nimbus to follow you around");
		}

		public override void SetDefaults()
		{
			item.damage = 0;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.width = 20;
			item.height = 26;
			item.UseSound = SoundID.Item44;
			item.useAnimation = 20;
			item.useTime = 20;
			item.rare = ItemRarityID.Blue;
			item.noMelee = true;
			item.value = Item.sellPrice(silver: 60);
			item.buffType = ModContent.BuffType<NimbusPetBuff>();
		}

		public override void UseStyle(Player player)
		{
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
				player.AddBuff(item.buffType, 20000, true);
		}
	}
}
