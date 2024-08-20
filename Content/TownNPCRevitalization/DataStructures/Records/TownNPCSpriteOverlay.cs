using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

/// <summary>
///     Simple "wrapper" for a <see cref="Texture2D" /> asset that has an associated position.
/// </summary>
public record TownNPCSpriteOverlay(Texture2D Texture, Point PositionInFrame) : IDisposable {
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if (disposing) {
            Texture?.Dispose();
        }
    }
}