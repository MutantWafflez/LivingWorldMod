using LivingWorldMod.Content.Items.Pets;
using LivingWorldMod.Content.Items.Placeables.Building;
using LivingWorldMod.Content.Items.Placeables.Furniture.Critter;
using LivingWorldMod.Content.Items.Placeables.Furniture.Harpy;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.Content.NPCs.Villagers {
    public class HarpyVillager : Villager {
        public override VillagerType VillagerType => VillagerType.Harpy;

        public override WeightedRandom<ShopItem> ShopPool {
            get {
                WeightedRandom<ShopItem> pool = new WeightedRandom<ShopItem>();
                VillagerRelationship relationship = RelationshipStatus;

                //Furniture & Blocks
                pool.Add(new ShopItem(ModContent.ItemType<GravityTapestryItem>(), 3, null));
                pool.Add(new ShopItem(ModContent.ItemType<SunTapestryItem>(), 3, null));
                pool.Add(new ShopItem(ModContent.ItemType<StarshardCloudItem>(), 75, null));
                pool.Add(new ShopItem(ModContent.ItemType<StarshineBlockItem>(), 75, null));
                pool.Add(new ShopItem(ModContent.ItemType<SkywareBlockItem>(), 75, null));
                pool.Add(new ShopItem(ModContent.ItemType<SkywareLoomItem>(), 3, null));
                pool.Add(new ShopItem(ModContent.ItemType<SkywareAnvilItem>(), 3, null));
                pool.Add(new ShopItem(ItemID.SkywareChair, 3, null));
                pool.Add(new ShopItem(ItemID.SkywareTable, 3, null));
                pool.Add(new ShopItem(ItemID.SkyMill, 1, null));
                pool.AddConditionally(new ShopItem(ModContent.ItemType<NimbusJarItem>(), 2, null), relationship >= VillagerRelationship.Like);

                //Weapons & Accessories
                pool.AddConditionally(new ShopItem(ItemID.ShinyRedBalloon, 1, Item.buyPrice(gold: 5)), relationship >= VillagerRelationship.Like);
                pool.AddConditionally(new ShopItem(ItemID.Starfury, 1, Item.buyPrice(gold: 5)), relationship >= VillagerRelationship.Like);

                //Vanity/Pets
                pool.AddConditionally(new ShopItem(ModContent.ItemType<NimbusInABottle>(), 2, null), relationship >= VillagerRelationship.Like);

                //Materials
                pool.AddConditionally(new ShopItem(ItemID.WhitePearl, 3, Item.buyPrice(gold: 1)), relationship >= VillagerRelationship.Like);
                pool.AddConditionally(new ShopItem(ItemID.BlackPearl, 3, Item.buyPrice(gold: 5)), relationship >= VillagerRelationship.Like, 0.5f);
                pool.AddConditionally(new ShopItem(ItemID.PinkPearl, 3, Item.buyPrice(gold: 15)), relationship == VillagerRelationship.Love, 0.5f);

                //Worms
                pool.Add(new ShopItem(ItemID.Worm, 10, Item.buyPrice(silver: 2)));
                pool.Add(new ShopItem(ItemID.EnchantedNightcrawler, 5, Item.buyPrice(silver: 50)), 0.67f);
                pool.Add(new ShopItem(ItemID.GoldWorm, 1, Item.buyPrice(gold: 15)), relationship < VillagerRelationship.Like ? 0.1f : 0.15f);

                return pool;
            }
        }

        public override void SetStaticDefaults() {
            Main.npcFrameCount[Type] = 27;
            NPCID.Sets.ExtraFramesCount[Type] = 6;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,

                new FlavorTextBestiaryInfoElement("Mods.LivingWorldMod.Bestiary.HarpyVillager")
            });
        }
    }
}