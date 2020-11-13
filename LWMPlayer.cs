using LivingWorldMod.Projectiles.Friendly;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod
{
    public class LWMPlayer : ModPlayer
    {
        //Accessory Bools
        public bool featherBag;
        //Other Accessory Variables
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
           if (--timeUntilNextFeather <= 0) {
                timeUntilNextFeather = 0;
           }

            if (featherBag == true && Main.myPlayer == player.whoAmI)
            {
                 if (player.justJumped == true)
                 {
                    int numberProjectiles;
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
                        int feather = Projectile.NewProjectile(player.Bottom - new Vector2(0, 5), perturbedSpeed, ModContent.ProjectileType<FeatherBagFeather>(), 18, 2.5f, player.whoAmI);
                        NetMessage.SendData(MessageID.SyncProjectile, number: feather);
                    }
                }
                if (player.wingTime < player.wingTimeMax && player.wingTime != 0 && player.velocity.Y < 0f)
                {
                    int numberProjectiles = 12;
                    float rotation = MathHelper.ToRadians(180);

                    if (timeUntilNextFeather == 0)
                    {
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = new Vector2(6, 6).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)numberProjectiles));
                            int feather = Projectile.NewProjectile(player.Bottom - new Vector2(0, 5), perturbedSpeed, ModContent.ProjectileType<FeatherBagFeather>(), 18, 2.5f, player.whoAmI);
                            NetMessage.SendData(MessageID.SyncProjectile, number: feather);
                        }
                        timeUntilNextFeather = 90;
                    }
                }
            }
            #endregion
        }
    }
}
