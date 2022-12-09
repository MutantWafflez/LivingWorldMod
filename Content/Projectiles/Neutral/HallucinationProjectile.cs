using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.Projectiles.Neutral {
    /// <summary>
    /// A projectile that acts as a "hallucination." It deals
    /// no damage and deletes itself over a specified period of time or
    /// with collision. Only the client that spawned the projectile
    /// can see it.
    /// </summary>
    public class HallucinationProjectile : BaseProjectile {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.WoodenArrowFriendly;

        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.hide = false;
            Projectile.alpha = 255;

            AIType = ProjectileID.WoodenArrowFriendly;
        }

        public override bool PreDraw(ref Color lightColor) {
            if (Main.myPlayer != Projectile.owner) {
                return false;
            }

            return true;
        }

        public override void AI() {
            if ((Projectile.alpha -= 20) <= 0) {
                Projectile.alpha = 0;
            }
        }
    }
}