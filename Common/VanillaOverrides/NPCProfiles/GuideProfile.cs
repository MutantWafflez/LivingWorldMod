using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.VanillaOverrides.NPCProfiles {
    /// <summary>
    /// Profile class for specifically the guide.
    /// </summary>
    public sealed class GuideProfile : ITownNPCProfile {
        public int RollVariation() => 0;

        public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

        public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) {
            if (Main.raining) {
                Asset<Texture2D> rainVariant = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}NPCProfiles/Guide_Rain");
                npc.frame = new Rectangle(npc.frame.X, npc.frame.Y, rainVariant.Width(), rainVariant.Height() / Main.npcFrameCount[npc.type]);

                return rainVariant;
            }

            npc.frame = new Rectangle(npc.frame.X, npc.frame.Y, TextureAssets.Npc[npc.type].Width(), TextureAssets.Npc[npc.type].Height() / Main.npcFrameCount[npc.type]);
            return TextureAssets.Npc[npc.type];
        }

        public int GetHeadTextureIndex(NPC npc) => NPC.TypeToDefaultHeadIndex(npc.type);
    }
}