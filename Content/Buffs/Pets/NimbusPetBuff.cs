using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Projectiles.Friendly.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Buffs.Pets {
    //Thanks Trivaxy for the code! :-)
    public class NimbusPetBuff : BaseBuff {
        public override void SetStaticDefaults() {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) {
            player.buffTime[buffIndex] = 20000; // stop the buff from expiring on its own
            player.GetModPlayer<PetPlayer>().nimbusPet = true; // keep the bool active

            int nimbusProjectileID = ModContent.ProjectileType<NimbusPetProjectile>();
            bool nimbusSpawned = player.ownedProjectileCounts[nimbusProjectileID] > 0;

            if (!nimbusSpawned && player.whoAmI == Main.myPlayer) {
                for (int i = 0; i < 15; i++) {
                    Dust.NewDustPerfect(player.Center, 16, Main.rand.NextVector2Unit() * 3, Scale: Main.rand.NextFloat(0.8f, 1.5f));
                }

                Projectile.NewProjectile(new ProjectileSource_Buff(player, Type, buffIndex),player.Center - Vector2.UnitY * 5, Vector2.UnitX * player.direction * 5, nimbusProjectileID, 0, 0, player.whoAmI);
            }
        }
    }
}