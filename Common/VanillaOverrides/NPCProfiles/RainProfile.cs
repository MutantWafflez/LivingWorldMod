using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.VanillaOverrides.NPCProfiles;

/// <summary>
/// Profile class for specifically NPCs who are out in the rain.
/// </summary>
public sealed class RainProfile : ITownNPCProfile {
    public int RollVariation() => 0;

    public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

    public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) {
        if (!npc.IsABestiaryIconDummy && Main.raining && npc.Center.Y < Main.rockLayer * 16f && npc.Center.Y > Main.worldSurface * 0.35f * 16f && !NPCUtils.IsEntityUnderRoof(npc)) {
            Asset<Texture2D> rainVariant = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}NPCProfiles/Guide_Rain");
            npc.frame.Width = rainVariant.Width();
            npc.frame.Height = rainVariant.Height() / Main.npcFrameCount[npc.type];
            if (npc.frame.Y % npc.frame.Height != 0) {
                npc.frame.Y = 0;
            }

            return rainVariant;
        }

        npc.frame.Width = TextureAssets.Npc[npc.type].Width();
        npc.frame.Height = TextureAssets.Npc[npc.type].Height() / Main.npcFrameCount[npc.type];
        if (npc.frame.Y % npc.frame.Height != 0) {
            npc.frame.Y = 0;
        }
        //Using the default TextureAssets.Npc[npc.type] for some reason doesn't change anything, and here I am manually requesting. Eh??
        return Main.Assets.Request<Texture2D>("Images/NPC_22");
    }

    public int GetHeadTextureIndex(NPC npc) => NPC.TypeToDefaultHeadIndex(npc.type);
}