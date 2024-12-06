using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

/// <summary>
///     Data structure representing a request to draw the specified texture, where each field can be overriden by the default <see cref="DrawData" /> that is created when drawing a given Town NPC.
/// </summary>
/// <param name="Texture"> The texture that is going to be drawn. </param>
/// <param name="Position">
///     The position relative to the default draw position of the NPC. If <see cref="UsesAbsolutePosition" /> is <b>true</b>, then this value will be used as is, ignoring the default
///     draw data position.
/// </param>
/// <param name="SourceRectangle"> If null, is replaced by the default <see cref="DrawData" />. </param>
/// <param name="Color">If null, is replaced by the default <see cref="DrawData" />.</param>
/// <param name="Rotation">If null, is replaced by the default <see cref="DrawData" />.</param>
/// <param name="Origin">If null, is replaced by the default <see cref="DrawData" />.</param>
/// <param name="Scale">If null, is replaced by the default <see cref="DrawData" />.</param>
/// <param name="SpriteEffect">If null, is replaced by the default <see cref="DrawData" />.</param>
/// <param name="UsesAbsolutePosition"> Whether or not to treat the <see cref="Position" /> parameter "as-is." See that parameter for more info. </param>
/// <param name="DrawLayer"> What layer this request is going to be drawn on: lower numbers are drawn first, higher number later. </param>
public readonly record struct TownNPCDrawRequest(
    Texture2D Texture,
    Vector2 Position,
    Rectangle? SourceRectangle = null,
    Color? Color = null,
    float? Rotation = null,
    Vector2? Origin = null,
    Vector2? Scale = null,
    SpriteEffects? SpriteEffect = null,
    bool UsesAbsolutePosition = false,
    int DrawLayer = 0
) : IComparable<TownNPCDrawRequest> {
    public TownNPCDrawRequest(Texture2D texture) : this(texture, Vector2.Zero) { }

    /// <summary>
    ///     "Unionizes" this DrawRequest with a <see cref="DrawData" />, where every DrawRequest field is overriden if it is null.
    /// </summary>
    public DrawData UnionWithDrawData(DrawData data) => new (
        Texture,
        UsesAbsolutePosition ? Position : data.position + Position,
        SourceRectangle,
        Color ?? data.color,
        Rotation ?? data.rotation,
        Origin ?? data.origin,
        Scale ?? data.scale,
        SpriteEffect ?? data.effect
    );

    public int CompareTo(TownNPCDrawRequest other) => DrawLayer.CompareTo(other.DrawLayer);
}