using System.Collections.Generic;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.Systems.BaseSystems;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace LivingWorldMod.Common.Systems;

/// <summary>
/// Mod system that handles miscellaneous loading tasks that otherwise
/// don't deserve their own mod system.
/// </summary>
public class LoadingTaskSystem : BaseModSystem<LoadingTaskSystem> {
    public override void PostSetupContent() {
        Main.QueueMainThreadAction(GenerateTalkBlinkTextures);
    }

    private void GenerateTalkBlinkTextures() {
        Dictionary<int, (Texture2D, Texture2D)> talkBlinkOverlays = new();
        NPC npc = new();
        TownGlobalNPC townSingletonNPC = ModContent.GetInstance<TownGlobalNPC>();
        ILog logger = ModContent.GetInstance<LWM>().Logger;
        for (int i = 0; i < NPCLoader.NPCCount; i++) {
            npc.SetDefaults(i);
            if (!townSingletonNPC.AppliesToEntity(npc, true) || !TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile)) {
                continue;
            }

            Asset<Texture2D> npcAsset = profile.GetTextureNPCShouldUse(npc);
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