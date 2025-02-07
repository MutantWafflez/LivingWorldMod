using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

/// <summary>
///     Data type that represents a <see cref="Texture2D" /> instance that will be drawn over a Town NPC with a provided offset, with any potential amounts of additional offset from a given a
///     specific animation frame. In short, the provided overlay texture will be drawn by the given <see cref="DefaultDrawOffset" /> and offset further if the given frame has a key/value in
///     <see cref="AdditionalFrameOffsets" />.
/// </summary>
public record struct TownNPCSpriteOverlay(Texture2D Texture, Vector2 DefaultDrawOffset, Dictionary<int, Vector2> AdditionalFrameOffsets) {
    public TownNPCSpriteOverlay(Texture2D texture, Vector2 defaultDrawOffset) : this(texture, defaultDrawOffset, []) { }
}