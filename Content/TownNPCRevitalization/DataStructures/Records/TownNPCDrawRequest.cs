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
/// <param name="Origin">The rotational center of this overlay. Used identically to a normal <see cref="SpriteBatch" /> Draw call. If null, is replaced by the default <see cref="DrawData" />. </param>
/// <param name="SourceRectangle"> If null, is replaced by the default <see cref="DrawData" />. </param>
/// <param name="Color">If null, is replaced by the default <see cref="DrawData" />.</param>
/// <param name="Rotation">If null, is replaced by the default <see cref="DrawData" />.</param>
/// <param name="Scale">If null, is replaced by the default <see cref="DrawData" />.</param>
/// <param name="SpriteEffect">If null, is replaced by the default <see cref="DrawData" />.</param>
/// <param name="DrawLayer"> What layer this request is going to be drawn on: lower numbers are drawn first, higher number later. </param>
/// <param name="UsesAbsolutePosition">
///     Whether or not to treat the <see cref="Position" /> parameter "as-is" instead of adding it to the default <see cref="DrawData" />'s Position value. See that
///     parameter for more info.
/// </param>
/// <param name="UsesAbsoluteOrigin">
///     Whether or not to treat the <see cref="Origin" /> parameter "as-is" instead of adding it to the default <see cref="DrawData" />'s origin value. See that parameter
///     for more info. Note: If the <see cref="Origin" /> parameter is not passed in (i.e. is null), this parameter effectively does nothing.
/// </param>
public readonly record struct TownNPCDrawRequest(
    Texture2D Texture,
    Vector2 Position,
    Vector2? Origin = null,
    Rectangle? SourceRectangle = null,
    Color? Color = null,
    float? Rotation = null,
    Vector2? Scale = null,
    SpriteEffects? SpriteEffect = null,
    int DrawLayer = 0,
    bool UsesAbsolutePosition = false,
    bool UsesAbsoluteOrigin = false
) : IComparable<TownNPCDrawRequest> {
    public TownNPCDrawRequest(Texture2D texture) : this(texture, Vector2.Zero) { }

    /// <summary>
    ///     "Unionizes" this DrawRequest with a <see cref="DrawData" />, where every DrawRequest field is overriden if it is null.
    /// </summary>
    public DrawData UnionWithDrawData(DrawData data, Vector2 screenPos) {
        Vector2 origin = UsesAbsoluteOrigin ? Origin ?? data.origin : data.origin + (Origin ?? Vector2.Zero);

        return new DrawData (
            Texture,
            (UsesAbsolutePosition ? Position : data.position + Position + origin) - screenPos,
            SourceRectangle,
            Color ?? data.color,
            Rotation ?? data.rotation,
            origin,
            Scale ?? data.scale,
            SpriteEffect ?? data.effect
        );
    }

    public int CompareTo(TownNPCDrawRequest other) => DrawLayer.CompareTo(other.DrawLayer);
}