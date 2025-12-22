using System;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;

/// <summary>
///     Class that acts similarly to a class that implements <see cref="ITownNPCProfile" /> which stores data on "sprite overlays" for Town NPCs, namely for usage with the auto-generated blinking and
///     talking textures.
/// </summary>
public class OverlayProfile(params TownNPCSpriteOverlay[][] overlays) : IDisposable {
    public TownNPCSpriteOverlay GetCurrentSpriteOverlay(NPC npc, int overlayIndex) => overlays[npc.townNpcVariationIndex < overlays.Length ? npc.townNpcVariationIndex : 0][overlayIndex];

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if (!disposing) {
            return;
        }

        foreach (TownNPCSpriteOverlay[] overlayGroup in overlays) {
            foreach (TownNPCSpriteOverlay overlay in overlayGroup) {
                overlay.Texture.Dispose();
            }
        }
    }
}