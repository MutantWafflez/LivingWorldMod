using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hjson;
using LivingWorldMod.Content.Villages.DataStructures.Classes;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.Globals.Systems;
using LivingWorldMod.Content.Villages.Globals.Systems.UI;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;

/// <summary>
/// Base class for all the villager NPCs in the mod. Has several properties that can be
/// modified depending on the "personality" of the villagers.
/// </summary>
public abstract class Villager : ModNPC {
    /// <summary>
    /// A dictionary holding all villager names for each type.
    /// </summary>
    public static IReadOnlyDictionary<VillagerType, IReadOnlyList<string>> villagerNames;

    public sealed override string Texture => LWM.SpritePath + $"NPCs/Villagers/{VillagerType}/DefaultStyle";

    public override bool IsCloneable => true;

    /// <summary>
    /// What type of villager this class pertains to. Vital for several functions in the class
    /// and must be defined.
    /// </summary>
    public abstract VillagerType VillagerType {
        get;
    }

    /// <summary>
    /// A list of ALL POSSIBLE shop items that villagers of this given type can ever sell. This
    /// list is checked upon every restock.
    /// </summary>
    public abstract WeightedRandom<ShopItem> ShopPool {
        get;
    }

    /// <summary>
    /// Shorthand get property for acquiring the current relationship status of whatever type of village this villager belongs
    /// to.
    /// </summary>
    public VillagerRelationship RelationshipStatus => ReputationSystem.Instance.GetVillageRelationship(VillagerType);

    /// <summary>
    /// The array of indices that denote the sprite differences for this villager.
    /// </summary>
    public int[] DrawIndices {
        get;
        protected set;
    }

    /// <summary>
    /// The draw object used to draw this villager.
    /// </summary>
    public LayeredDrawObject drawObject;

    /// <summary>
    /// A list of shop items that this specific villager is selling at this very moment.
    /// </summary>
    public List<ShopItem> shopInventory;

    /// <summary>
    /// A counter for how long this Villager has been homeless for, used for automatically leaving
    /// </summary>
    private int _homelessCounter;

    public override ModNPC NewInstance(NPC entity) {
        Villager instance = (Villager)base.NewInstance(entity);

        instance.RestockShop();
        instance.RandomizeFeatures();

        return instance;
    }

    public override ModNPC Clone(NPC newEntity) {
        Villager clone = (Villager)base.Clone(newEntity);

        clone.shopInventory = shopInventory;
        clone.drawObject = drawObject;
        clone._homelessCounter = _homelessCounter;

        return clone;
    }

    public sealed override void AutoStaticDefaults() {
        base.AutoStaticDefaults();

        //The PR is here, and I am loving it 
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        NPCID.Sets.SpawnsWithCustomName[Type] = true;
        NPCID.Sets.AllowDoorInteraction[Type] = true;
        // Villagers use TownNPC AI, which makes this necessary
        NPCID.Sets.ShimmerTownTransform[Type] = true;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() {
            Velocity = 1f,
            Direction = -1
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        // Villager Names
        Dictionary<VillagerType, IReadOnlyList<string>> tempVillagerNames = new();
        JsonValue villagerNameData = LWMUtils.GetJSONFromFile("Assets/JSONData/VillagerNames.json");

        for (VillagerType villagerType = 0; (int)villagerType < LWMUtils.GetTotalVillagerTypeCount(); villagerType++) {
            tempVillagerNames[villagerType] = villagerNameData[villagerType.ToString()].Qa().Select(value => value.Qs()).ToList();
        }

        villagerNames = tempVillagerNames;
    }

    public override void SetDefaults() {
        NPC.width = 25;
        NPC.height = 40;
        NPC.friendly = RelationshipStatus != VillagerRelationship.Hate;
        NPC.lifeMax = 500;
        NPC.defense = 15;
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = 7;
        AnimationType = NPCID.Guide;
    }

    public override bool NeedSaving() => true;

    public override void SaveData(TagCompound tag) {
        tag["LayerIndices"] = DrawIndices;
        tag["Shop"] = shopInventory;

        tag["DisplayName"] = NPC.GivenName;
        tag["HomeTileX"] = NPC.homeTileX;
        tag["HomeTileY"] = NPC.homeTileY;
        tag["IsHomeless"] = NPC.homeless;
    }

    public override void LoadData(TagCompound tag) {
        if (tag.TryGet("LayerIndices", out int[] indices)) {
            DrawIndices = indices;
        }
        else {
            RandomizeFeatures();
        }
        shopInventory = tag.Get<List<ShopItem>>("Shop");

        NPC.GivenName = tag.GetString("DisplayName");
        NPC.homeTileX = tag.GetInt("HomeTileX");
        NPC.homeTileY = tag.GetInt("HomeTileY");
        NPC.homeless = tag.GetBool("IsHomeless");
    }

    public override bool CheckActive() => false;

    public override List<string> SetNPCNameList() => villagerNames[VillagerType].ToList();

    public override void SetChatButtons(ref string button, ref string button2) {
        button = Language.GetTextValue("LegacyInterface.28"); //"Shop"
        button2 = LWMUtils.GetLWMTextValue("Common.Reputation");
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shopName) {
        //Shop Screen
        if (firstButton) {
            ModContent.GetInstance<ShopUISystem>().OpenShopUI(this);
        }
        //Reputation Screen
    }

    public override bool CanChat() => RelationshipStatus != VillagerRelationship.Hate;

    public sealed override string GetChat() {
        if (RelationshipStatus == VillagerRelationship.Hate) {
            return "..."; //The player should be unable to chat with the villagers if they are hated, but just in case that occurs, return something in order to prevent an error thrown
        }

        return DialogueSystem.Instance.GetDialogue(VillagerType, RelationshipStatus, DialogueType.Normal);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        drawObject.Draw(
            spriteBatch,
            new DrawData(
                null,
                new Rectangle(
                    (int)(NPC.Right.X - NPC.frame.Width / 1.5 - screenPos.X),
                    (int)(NPC.Bottom.Y - NPC.frame.Height - screenPos.Y + 2f),
                    NPC.frame.Width,
                    NPC.frame.Height
                ),
                NPC.frame,
                NPC.GetShimmerColor(drawColor),
                NPC.rotation,
                default(Vector2),
                NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
            ),
            DrawIndices
        );

        return false;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        int layerCount = DrawIndices.Length;

        writer.Write(layerCount);
        for (int i = 0; i < layerCount; i++) {
            writer.Write(DrawIndices[i]);
        }
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        int layerCount = reader.ReadInt32();

        for (int i = 0; i < layerCount; i++) {
            DrawIndices[i] = reader.ReadInt32();
        }
    }

    public override void PostAI() {
        //We only want this to run on Server/SP
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        // This means that the villager has been shimmered
        if (NPC.townNpcVariationIndex == 1) {
            RandomizeFeatures();
            NPC.townNpcVariationIndex = 0;

            NPC.netUpdate = true;
        }

        if (NPC.homeless) {
            _homelessCounter++;
        }
        else {
            _homelessCounter = 0;
            return;
        }

        if (_homelessCounter >= 60 * 60 * 2) {
            Color leavingColor = new(255, 25, 25);
            string leavingText = LWMUtils.GetLWMTextValue($"Event.VillagerLeft.{VillagerType}", NPC.GivenOrTypeName);

            NPC.active = false;
            if (Main.netMode == NetmodeID.Server) {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(leavingText), leavingColor);
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }
            else {
                Main.NewText(leavingText, leavingColor);
                SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);
            }
        }
    }

    /// <summary>
    /// Restocks the shop of this villager, drawing from the SpawnPool property.
    /// </summary>
    public void RestockShop() {
        shopInventory = [];

        int shopLength = Main.rand.Next(6, 8);

        do {
            ShopItem returnedItem = ShopPool;

            if (shopInventory.All(item => item != returnedItem)) {
                shopInventory.Add(returnedItem);
            }
        } while (shopInventory.Count < shopLength);
    }

    /// <summary>
    /// Randomizes all features of this villager.
    /// </summary>
    public void RandomizeFeatures() {
        for (int i = 0; i < DrawIndices.Length; i++) {
            DrawIndices[i] = Main.rand.Next(drawObject.GetLayerVariations(i));
        }
    }
}