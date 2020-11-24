using LivingWorldMod.Items.Extra;
using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.Projectiles.Friendly;
using LivingWorldMod.Tiles.Interactables;
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
        public int clampTheValueForFeather = 60;
        int timeUntilNextFeather;

        public override void PostItemCheck()
        {
            if (player.HeldItem.type == ModContent.ItemType<RoseMirror>() && player.itemAnimation > 0)
            {
                if (Main.rand.Next(2) == 0)
                {
                    Dust.NewDust(player.position, player.width, player.height, DustID.PinkCrystalShard, 0f, 0f, 150, default, 1.1f);
                }
                if (player.itemAnimation == player.HeldItem.useAnimation / 2)
                { 
                    for (int j = 0; j < 70; j++)
                    {
                        Dust.NewDust(player.position, player.width, player.height, DustID.PinkCrystalShard, player.velocity.X * 0.5f, player.velocity.Y * 0.5f, 150, default, 1.5f);
                    }
                    if (LWMWorld.shrineCoords[(int)VillagerType.Harpy] != Vector2.Zero) {
                        player.UnityTeleport(LWMWorld.shrineCoords[(int)VillagerType.Harpy] * 16 + new Vector2(player.width, player.height - 5));
                        for (int k = 0; k < 70; k++) {
                            Dust.NewDust(player.position, player.width, player.height, DustID.PinkCrystalShard, 0f, 0f, 150, default, 1.5f);
                        }
                    }
                }
            }
        }

        public override void ResetEffects()
        {
            featherBag = false;
        }

        public override void UpdateDead()
        {
            timeUntilNextFeather = clampTheValueForFeather;
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
                    int numberProjectiles = 5;
                    float rotation = MathHelper.ToRadians(180);

                    if (timeUntilNextFeather == 0)
                    {
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = new Vector2(6, 6).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)numberProjectiles));
                            int feather = Projectile.NewProjectile(player.Bottom - new Vector2(0, 5), perturbedSpeed, ModContent.ProjectileType<FeatherBagFeather>(), 18, 2.5f, player.whoAmI);
                            NetMessage.SendData(MessageID.SyncProjectile, number: feather);
                        }
                        timeUntilNextFeather = clampTheValueForFeather;
                    }
                }
            }
            #endregion
        }
    }
}
