using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Globals.Systems.BaseSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     Mod system that handles more "precise" loading tasks for the Town NPC revitilization, beyond just Load() and SetStaticDefaults().
/// </summary>
public class TownNPCRevitalizationLoadSystem : BaseModSystem<TownNPCRevitalizationLoadSystem> {
    public override void PostSetupContent() {
        if (Main.netMode != NetmodeID.Server) {
            Main.QueueMainThreadAction(GenerateTalkBlinkTextures);
        }

        TownNPCMoodModule.Load();
    }

    private void GenerateTalkBlinkTextures() {
        Dictionary<int, (Texture2D, Texture2D)> talkBlinkOverlays = [];
        NPC npc = new();
        TownGlobalNPC townSingletonNPC = ModContent.GetInstance<TownGlobalNPC>();
        for (int i = 0; i < NPCLoader.NPCCount; i++) {
            npc.SetDefaults(i);
            if (!townSingletonNPC.AppliesToEntity(npc, true)) {
                continue;
            }

            Asset<Texture2D> npcAsset = TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile) ? profile.GetTextureNPCShouldUse(npc) : TextureAssets.Npc[i];
            string modName = "Terraria";
            if (i >= NPCID.Count) {
                modName = NPCLoader.GetNPC(i).Mod.Name;
            }

            Texture2D npcTexture = ModContent.Request<Texture2D>($"{modName}/{npcAsset.Name}".Replace("\\", "/"), AssetRequestMode.ImmediateLoad).Value;
            int npcFrameHeight = npcTexture.Height / Main.npcFrameCount[i];
            int totalPixelArea = npcTexture.Width * npcFrameHeight;
            int nonAttackFrameCount = Main.npcFrameCount[i] - NPCID.Sets.AttackFrameCount[i];

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
            talkingFrameTexture.Name = $"{npcAsset.Name[(npcAsset.Name.LastIndexOf("\\") + 1)..]}_Talking";

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
            blinkingFrameTexture.Name = $"{npcAsset.Name[(npcAsset.Name.LastIndexOf("\\") + 1)..]}_Blinking";

            talkBlinkOverlays[i] = (talkingFrameTexture, blinkingFrameTexture);
        }

        TownGlobalNPC.talkBlinkOverlays = talkBlinkOverlays;
    }
}