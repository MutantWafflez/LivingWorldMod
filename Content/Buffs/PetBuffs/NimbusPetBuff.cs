using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Projectiles.Friendly.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Buffs.PetBuffs
{
    public class NimbusPetBuff : ModBuff
    {
        public override void SetDefaults()
        {
            // TODO: Localization?
            DisplayName.SetDefault("Nimbus Pet");
            Description.SetDefault("I bet it tastes like cotton candy...");
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 20000; // stop the buff from expiring on its own
            player.GetModPlayer<LWMPlayer>().nimbusPet = true; // keep the bool active

            int nimbusProjectileID = ModContent.ProjectileType<NimbusPetProjectile>();
            bool nimbusSpawned = player.ownedProjectileCounts[nimbusProjectileID] > 0;

            if (!nimbusSpawned && player.whoAmI == Main.myPlayer)
            {
                for (int i = 0; i < 15; i++)
                    Dust.NewDustPerfect(player.Center, 16, Main.rand.NextVector2Unit() * 3, Scale: Main.rand.NextFloat(0.8f, 1.5f));
                Projectile.NewProjectile(player.Center - Vector2.UnitY * 5, Vector2.UnitX * player.direction * 5, nimbusProjectileID, 0, 0, player.whoAmI);
            }
        }
    }
}