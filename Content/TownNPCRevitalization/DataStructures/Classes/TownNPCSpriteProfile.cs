using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;

/// <summary>
///     Class that acts similarly to a class that implements <see cref="ITownNPCProfile" /> which stores data on "sprite overlays" for Town NPCs, namely for usage with the auto-generated blinking and
///     talking textures.
/// </summary>
public class TownNPCSpriteProfile(params Texture2D[][] overlays) : IDisposable {
    public Texture2D GetCurrentSpriteOverlay(NPC npc, int overlayIndex) => overlays[npc.townNpcVariationIndex < overlays.Length ? npc.townNpcVariationIndex : 0][overlayIndex];

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if (!disposing) {
            return;
        }

        foreach (Texture2D[] overlayGroup in overlays) {
            foreach (Texture2D overlay in overlayGroup) {
                overlay.Dispose();
            }
        }
    }
}