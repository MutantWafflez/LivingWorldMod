using LivingWorldMod.ID;
using LivingWorldMod.Items.Extra;
using LivingWorldMod.Items.Placeable.Paintings;
using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.Projectiles.Friendly;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Reflection;
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

        #region Feather Bag

        /// <summary>
        /// Maximum time (in ticks) between each feather burst when flying or initially jumping
        /// </summary>
        public static readonly int maxFeatherTimer = 60;

        /// <summary>
        /// The time between feathers while in flight
        /// </summary>
        private int timeUntilNextFeather;

        /// <summary>
        /// The amount of time (in ticks) between each jump that must pass before the feather burst can trigger again
        /// </summary>
        private int featherBagJumpCooldown;

        /// <summary>
        /// Array for all the possible types of vanilla wings to create the different visuals of the feather bag
        /// </summary>
        public static readonly FieldInfo[] wingList = typeof(ArmorIDs.Wing).GetFields().Where(field => field.IsLiteral && !field.IsInitOnly).ToArray();

        #endregion Feather Bag
        
        /// <summary>
        /// A cache of the item index as found during CanBuyItem. Used in PostBuyItem to update the item slot after
        /// buying, and put back the item but with an X over it, when the stack runs out.
        /// </summary>
        private int itemIdx;

        public override void PostItemCheck()
        {
            RoseMirrorItem();
        }

        public override void ResetEffects()
        {
            featherBag = false;
        }

        public override void UpdateDead()
        {
            timeUntilNextFeather = maxFeatherTimer;
        }

        public override void PostUpdate()
        {
            FeatherBagAccessory();
        }

        #region Accessory Methods

        private void FeatherBagAccessory()
        {
            if (--timeUntilNextFeather <= 0)
            {
                timeUntilNextFeather = 0;
            }

            if (--featherBagJumpCooldown <= 0)
            {
                featherBagJumpCooldown = 0;
            }

            if (featherBag && Main.myPlayer == player.whoAmI)
            {
                int justStarted = 0;
                WingID wingType;
                if (player.wings != 0)
                {
                    wingType = EquippedWingsToID();
                }
                else
                {
                    wingType = WingID.Default;
                }

                int featherDamage;
                if (wingType <= WingID.SpookyWings)
                {
                    featherDamage = (int)Math.Ceiling(Math.Pow(Math.E, ((float)wingType * 0.25f) - 1f) + 20f); //e^((x*0.25) - 1) + 20f
                }
                else
                {
                    featherDamage = (int)Math.Ceiling(Math.Pow(Math.E, ((float)wingType * 0.15f) + 0.125f) + 31.5f); //e^((x*0.15) + 0.125) + 31.5
                }

                if (player.justJumped && featherBagJumpCooldown == 0)
                {
                    featherBagJumpCooldown = maxFeatherTimer;
                    int numberProjectiles;
                    if (player.wingTimeMax == justStarted)
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
                        int feather = Projectile.NewProjectile(player.Bottom - new Vector2(0, 5), perturbedSpeed, ModContent.ProjectileType<FeatherBagProjectile>(), featherDamage, 2.5f, player.whoAmI);
                        (Main.projectile[feather].modProjectile as FeatherBagProjectile).featherType = wingType;
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
                            int feather = Projectile.NewProjectile(player.Bottom - new Vector2(0, 5), perturbedSpeed, ModContent.ProjectileType<FeatherBagProjectile>(), featherDamage, 2.5f, player.whoAmI);
                            (Main.projectile[feather].modProjectile as FeatherBagProjectile).featherType = wingType;
                            NetMessage.SendData(MessageID.SyncProjectile, number: feather);
                        }
                        timeUntilNextFeather = maxFeatherTimer;
                    }
                }
            }
        }

        #endregion Accessory Methods

        #region Item Methods

        private void RoseMirrorItem()
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
                    if (LWMWorld.GetShrineTilePosition(VillagerType.Harpy) != Vector2.Zero)
                    {
                        player.UnityTeleport(LWMWorld.GetShrineWorldPosition(VillagerType.Harpy) + new Vector2(player.width, player.height - 5));
                        for (int k = 0; k < 70; k++)
                        {
                            Dust.NewDust(player.position, player.width, player.height, DustID.PinkCrystalShard, 0f, 0f, 150, default, 1.5f);
                        }
                    }
                }
            }
        }

        #endregion Item Methods

        #region Helper Methods

        private WingID EquippedWingsToID()
        {
            int currentWings = player.wings;
            foreach (FieldInfo wingInfo in wingList)
            {
                if ((sbyte)wingInfo.GetValue(null) == currentWings)
                {
                    if (Enum.TryParse(wingInfo.Name, true, out WingID returnValue))
                    {
                        return returnValue;
                    }
                    else
                    {
                        return WingID.Extra;
                    }
                }
            }
            return WingID.Default;
        }

        #endregion Helper Methods
        
        #region Item Slot Management
        
        public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item)
        {
            if (vendor.type != ModContent.NPCType<SkyVillager>())
                return;

            if (shopInventory[itemIdx].type == ItemID.None)
            {
                // restore the item to the slot and mark it as not purchasable
                shopInventory[itemIdx].SetDefaults(item.type);
                ((SkyBustTileItem) shopInventory[itemIdx].modItem).isOutOfStock = true;
            }

            item.buyOnce = false;
        }

        public override bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item)
        {
            // record the index of the item being purchased
            itemIdx = Array.IndexOf(shopInventory, item);
			
            // check if this is a modded item and cannot be purchased
            if (item.modItem is SkyBustTileItem tileItem && tileItem.isOutOfStock)
                return false;
			
            return true;
        }
        
        #endregion Item Slot Management
    }
}