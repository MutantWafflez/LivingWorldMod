using LivingWorldMod.Projectiles.Friendly;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod
{
    public class LWMPlayer : ModPlayer
    {
        public static bool featherBag;
        int timeUntilNextFeather = 90;

        public override void ResetEffects()
        {
            featherBag = false;
        }

        public override void UpdateDead()
        {
            timeUntilNextFeather = 90;
        }

        public override void PostUpdate()
        {
            #region Feather Bag
            timeUntilNextFeather--;

            if (featherBag == true)
            {
                if (player.justJumped == true)
                {
                    #pragma warning disable IDE0059 // Unnecessary assignment of a value
                    int numberProjectiles = 0;
                    #pragma warning restore IDE0059 // Unnecessary assignment of a value

                    if (player.wingTimeMax == 0)
                    {
                        numberProjectiles = 8; 
                    }
                    else
                    {
                        numberProjectiles = 12;
                    }

                    float rotation = MathHelper.ToRadians(180);
                    for (int i = 0; i < numberProjectiles; i++)
                    {
                        Vector2 perturbedSpeed = new Vector2(6, 6).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)numberProjectiles)); 
                        Projectile.NewProjectile(player.Bottom - new Vector2(0, 5), perturbedSpeed, ModContent.ProjectileType<FeatherBagFeather>(), 18, 2.5f, player.whoAmI);
                    }
                }
                if (player.wingTime < player.wingTimeMax && player.wingTime != 0 && player.velocity.Y != 0)
                {
                    int numberProjectiles = 12;
                    float rotation = MathHelper.ToRadians(180);

                    if (timeUntilNextFeather == 0)
                    {
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = new Vector2(6, 6).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)numberProjectiles));
                            Projectile.NewProjectile(player.Bottom - new Vector2(0, 5), perturbedSpeed, ModContent.ProjectileType<FeatherBagFeather>(), 18, 2.5f, player.whoAmI);
                        }
                        timeUntilNextFeather = 90;
                    }
                }
            }
            #endregion
        }
    }
}
