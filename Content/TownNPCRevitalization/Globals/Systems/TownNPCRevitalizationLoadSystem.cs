using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Globals.Systems.BaseSystems;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     Mod system that handles more "precise" loading tasks for the Town NPC revitalization, beyond just Load() and SetStaticDefaults().
/// </summary>
public class TownNPCRevitalizationLoadSystem : BaseModSystem<TownNPCRevitalizationLoadSystem> {
    private static TownNPCSpriteOverlay[] GenerateTownNPCSpriteOverlays(string npcAssetName, Texture2D npcTexture, int npcType) {
        int npcFrameHeight = npcTexture.Height / Main.npcFrameCount[npcType];
        int totalPixelArea = npcTexture.Width * npcFrameHeight;
        int nonAttackFrameCount = Main.npcFrameCount[npcType] - NPCID.Sets.AttackFrameCount[npcType];

        Color[] firstFrameData = new Color[totalPixelArea];
        Color[] secondFrameData = new Color[totalPixelArea];
        Rectangle frameRectangle = new(0, 0, npcTexture.Width, npcFrameHeight);

        //logger.Info($"Getting base pixel data for NPC \"{npc.TypeName}\"...");
        npcTexture.GetData(0, frameRectangle, firstFrameData, 0, totalPixelArea);

        //logger.Info($"Getting talk frame pixel data for NPC \"{npc.TypeName}\"...");
        npcTexture.GetData(0, frameRectangle with { Y = npcFrameHeight * (nonAttackFrameCount - 2) }, secondFrameData, 0, totalPixelArea);

        Color[] colorDifference = new Color[totalPixelArea];
        for (int j = 0; j < totalPixelArea; j++) {
            if (firstFrameData[j] != secondFrameData[j]) {
                colorDifference[j] = secondFrameData[j];
            }
        }

        Texture2D talkingFrameTexture = new(Main.graphics.GraphicsDevice, npcTexture.Width, npcFrameHeight);
        talkingFrameTexture.SetData(colorDifference);
        talkingFrameTexture.Name = $"{npcAssetName[(npcAssetName.LastIndexOf("\\") + 1)..]}_Talking";

        //logger.Info($"Getting blink frame pixel data for NPC \"{npc.TypeName}\"...");
        npcTexture.GetData(0, frameRectangle with { Y = npcFrameHeight * (nonAttackFrameCount - 1) }, secondFrameData, 0, totalPixelArea);
        for (int j = 0; j < totalPixelArea; j++) {
            colorDifference[j] = default(Color);
            if (firstFrameData[j] != secondFrameData[j]) {
                colorDifference[j] = secondFrameData[j];
            }
        }

        Texture2D blinkingFrameTexture = new(Main.graphics.GraphicsDevice, npcTexture.Width, npcFrameHeight);
        blinkingFrameTexture.SetData(colorDifference);
        blinkingFrameTexture.Name = $"{npcAssetName[(npcAssetName.LastIndexOf("\\") + 1)..]}_Blinking";

        return [new TownNPCSpriteOverlay(talkingFrameTexture, Point.Zero), new TownNPCSpriteOverlay(blinkingFrameTexture, Point.Zero)];
    }

    public override void PostSetupContent() {
        if (Main.netMode != NetmodeID.Server) {
            Main.QueueMainThreadAction(GenerateTownNPCSpriteProfiles);
        }

        TownNPCMoodModule.Load();
    }

    private void GenerateTownNPCSpriteProfiles() {
        Dictionary<int, TownNPCSpriteOverlayProfile> overlayProfiles = [];
        NPC npc = new();
        TownGlobalNPC townSingletonNPC = ModContent.GetInstance<TownGlobalNPC>();
        for (int i = 0; i < NPCLoader.NPCCount; i++) {
            npc.SetDefaults(i);
            if (!townSingletonNPC.AppliesToEntity(npc, true)) {
                continue;
            }

            string modName = i >= NPCID.Count ? NPCLoader.GetNPC(i).Mod.Name : "Terraria";
            Asset<Texture2D> npcAsset;
            if (!TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile)) {
                npcAsset = TextureAssets.Npc[i];
                overlayProfiles[i] = new TownNPCSpriteOverlayProfile(GenerateTownNPCSpriteOverlays(npcAsset.Name, npcAsset.ForceLoadAsset(modName), i));
                continue;
            }

            int npcVariationCount = profile is Profiles.StackedNPCProfile stackedNPCProfile ? stackedNPCProfile._profiles.Length : 1;
            List<TownNPCSpriteOverlay[]> spriteOverlays = [];
            for (int j = 0; j < npcVariationCount; npc.townNpcVariationIndex = ++j) {
                npcAsset = profile.GetTextureNPCShouldUse(npc);
                spriteOverlays.Add(GenerateTownNPCSpriteOverlays(npcAsset.Name, npcAsset.ForceLoadAsset(modName), i));
            }

            overlayProfiles[i] = new TownNPCSpriteOverlayProfile(spriteOverlays.ToArray());
        }

        TownNPCSpriteModule.overlayProfiles = overlayProfiles;
    }
}