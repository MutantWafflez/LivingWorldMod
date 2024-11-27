using System.Linq;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.DataStructures.Records;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.Villages.HarpyVillage.Pets;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Blocks;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;
using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture.Tapestries;
using LivingWorldMod.DataStructures.Classes;
using Terraria.GameContent.Bestiary;

namespace LivingWorldMod.Content.Villages.HarpyVillage.NPCs;

public class HarpyVillager : Villager {
    // Layer indices
    public const int BodyIndexID = 0;
    public const int OutfitIndexID = 1;
    public const int HairIndexID = 2;
    public const int FaceIndexID = 3;
    public const int WingsIndexID = 4;

    public override VillagerType VillagerType => VillagerType.Harpy;

    public override DynamicWeightedRandom<ShopItem> ShopPool {
        get {
            DynamicWeightedRandom<ShopItem> pool = new(Main.rand);
            VillagerRelationship relationship = RelationshipStatus;

            //Furniture & Blocks
            pool.Add(new ShopItem(ModContent.ItemType<GravityTapestryItem>(), 3));
            pool.Add(new ShopItem(ModContent.ItemType<SunTapestryItem>(), 3));
            pool.Add(new ShopItem(ModContent.ItemType<WorldTapestryItem>(), 3));
            pool.Add(new ShopItem(ModContent.ItemType<StormTapestryItem>(), 3));
            pool.Add(new ShopItem(ModContent.ItemType<StarshardCloudItem>(), 100));
            pool.Add(new ShopItem(ModContent.ItemType<StarshineBlockItem>(), 150));
            pool.Add(new ShopItem(ModContent.ItemType<SkywareBlockItem>(), 150));
            pool.Add(new ShopItem(ModContent.ItemType<SkywareLoomItem>(), 2));
            pool.Add(new ShopItem(ModContent.ItemType<SkywareAnvilItem>(), 2));
            pool.Add(new ShopItem(ItemID.SkywareChair, 10));
            pool.Add(new ShopItem(ItemID.SkywareTable, 5));
            pool.Add(new ShopItem(ItemID.SkyMill, 1));

            double likeConditionWeight = relationship >= VillagerRelationship.Like ? 1f : 0f;
            pool.Add(new ShopItem(ModContent.ItemType<NimbusJarItem>(), 2), likeConditionWeight);

            //Weapons & Accessories
            pool.Add(new ShopItem(ItemID.ShinyRedBalloon, 1, Item.buyPrice(gold: 10)), likeConditionWeight);
            pool.Add(new ShopItem(ItemID.Starfury, 1, Item.buyPrice(gold: 12, silver: 75)), likeConditionWeight);

            //Vanity/Pets
            pool.Add(new ShopItem(ModContent.ItemType<NimbusInABottle>(), 2), likeConditionWeight);

            //Materials
            pool.Add(new ShopItem(ItemID.WhitePearl, 3, Item.buyPrice(gold: 1)), likeConditionWeight);
            pool.Add(new ShopItem(ItemID.BlackPearl, 2, Item.buyPrice(gold: 5)), relationship >= VillagerRelationship.Like ? 0.5 : 0);
            pool.Add(new ShopItem(ItemID.PinkPearl, 1, Item.buyPrice(gold: 15)), relationship == VillagerRelationship.Love ? 0.25 : 0);

            //Worms
            pool.Add(new ShopItem(ItemID.Worm, 8, Item.buyPrice(silver: 10)));
            pool.Add(new ShopItem(ItemID.EnchantedNightcrawler, 3, Item.buyPrice(gold: 1)), 0.67);
            pool.Add(new ShopItem(ItemID.GoldWorm, 2, Item.buyPrice(gold: 15)), relationship >= VillagerRelationship.Love ? 0.2 : 0.15);

            return pool;
        }
    }

    public HarpyVillager() {
        string[] layerNames = ["Body", "Outfit", "Hair", "Face", "Wings"];

        DrawIndices = Enumerable.Repeat(0, layerNames.Length).ToArray();
        drawObject = new LayeredDrawObject(layerNames, LWM.SpritePath + "NPCs/Villagers/Harpy/", 5);
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 21;
        //NPCID.Sets.ExtraFramesCount[Type] = 6;

        NPC.buffImmune[BuffID.Suffocation] = true;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange(
            [ BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky, new FlavorTextBestiaryInfoElement("Mods.LivingWorldMod.Bestiary.HarpyVillager") ]
        );
    }
}