using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;

namespace LivingWorldMod.DataStructures.Classes;

/// <summary>
///     Represents a set of <see cref="Texture2D" /> objects that are
///     drawn in a certain order, and thus "layered" over each other.
/// </summary>
public class LayeredDrawObject {
    private record Layer(Asset<Texture2D>[] layerVariations, string name);

    private readonly Layer[] _layers;
    private readonly HashSet<string> _disabledLayers;

    /// <summary>
    ///     Creates a layered draw object with the associated names and variation counts,
    ///     acquiring assets from the given asset path with the name. Layer draw order
    ///     is determined by the order of the array.
    /// </summary>
    /// <remarks>
    ///     Assets follow the format of:
    ///     <code>
    /// $"{texturePath}{layerName}_{variationNumber}"
    /// </code>
    /// </remarks>
    public LayeredDrawObject((string, int)[] layerNameVariations, string texturePath) {
        _layers = new Layer[layerNameVariations.Length];

        for (int i = 0; i < layerNameVariations.Length; i++) {
            (string name, int variationCount) = layerNameVariations[i];

            Asset<Texture2D>[] layerVariations = new Asset<Texture2D>[variationCount];
            for (int j = 0; j < variationCount; j++) {
                layerVariations[j] = ModContent.Request<Texture2D>($"{texturePath}{name}_{j}");
            }

            _layers[i] = new Layer(layerVariations, name);
        }

        _disabledLayers = [];
    }

    /// <summary>
    ///     Creates this object with the assumption that all the layers have the same variation, and
    ///     thus only the name is required.
    /// </summary>
    /// <remarks>
    ///     Note that even with one variation, file/asset names still need to be appended with
    ///     the "_{num}" (see other construction for more info.)
    /// </remarks>
    public LayeredDrawObject(string[] layerNames, string texturePath, int variationCount = 1) : this(layerNames.Select(name => (name, variationCount)).ToArray(), texturePath) { }

    /// <summary>
    ///     Draws all non-disabled layers with the given <see cref="DrawData" />'s and
    ///     layer variations for each layer. To apply differences to the
    ///     <see cref="DrawData" />'s, use the <b>with</b> keyword.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, DrawData[] drawDatas, int[] layerVariations) {
        for (int i = 0; i < _layers.Length; i++) {
            ref Layer layer = ref _layers[i];
            if (_disabledLayers.Contains(layer.name)) {
                continue;
            }

            drawDatas[i].texture = layer.layerVariations[layerVariations[i]].Value;

            drawDatas[i].Draw(spriteBatch);
        }

        _disabledLayers.Clear();
    }

    /// <summary>
    ///     Draws all non-disabled layers with the same <see cref="DrawData" /> and
    ///     the specified layer variations.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, DrawData drawData, int[] layerVariations) => Draw(spriteBatch, Enumerable.Repeat(drawData, _layers.Length).ToArray(), layerVariations);

    /// <summary>
    ///     Draws all non-disabled layers with separate <see cref="DrawData" />'s and
    ///     the specified layer variation. To apply differences to the
    ///     <see cref="DrawData" />'s, use the <b>with</b> keyword.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, DrawData[] drawDatas, int layerVariation = 0) => Draw(spriteBatch, drawDatas, Enumerable.Repeat(layerVariation, _layers.Length).ToArray());

    /// <summary>
    ///     Draws all non-disabled layers with the same <see cref="DrawData" /> and
    ///     layer variation.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, DrawData drawData, int layerVariation = 0) => Draw(
        spriteBatch,
        Enumerable.Repeat(drawData, _layers.Length).ToArray(),
        Enumerable.Repeat(layerVariation, _layers.Length).ToArray()
    );

    /// <summary>
    ///     Disables the drawing of each layer associated with the passed
    ///     in layer names for the <b>NEXT DRAW CALL ONLY.</b>
    /// </summary>
    /// <param name="layerNames"> All layer names you want to disable. </param>
    public void DisableLayers(params string[] layerNames) {
        foreach (string name in layerNames) {
            _disabledLayers.Add(name);
        }
    }

    /// <summary>
    ///     Disables the drawing of each layer associated with the passed
    ///     in layer indices for the <b>NEXT DRAW CALL ONLY.</b>
    /// </summary>
    /// <param name="layerIndices"> All layer indices you want to disable. </param>
    public void DisableLayers(params int[] layerIndices) => DisableLayers(layerIndices.Select(index => _layers[index].name).ToArray());

    /// <summary>
    ///     Gets the associated layer and variation, then returns the frame width of
    ///     said variation.
    /// </summary>
    public int GetLayerFrameWidth(string layerName, int variation = 0, int frameCount = 1) => _layers.First(layer => layer.name == layerName).layerVariations[variation].Width() / frameCount;

    /// <inheritdoc cref="GetLayerFrameWidth(string,int, int)" />
    public int GetLayerFrameWidth(int layerIndex = 0, int variation = 0, int frameCount = 1) => _layers[layerIndex].layerVariations[variation].Width() / frameCount;

    /// <summary>
    ///     Gets the associated layer and variation, then returns the frame height of
    ///     said variation in relation to the passed in frame count.
    /// </summary>
    public int GetLayerFrameHeight(string layerName, int variation = 0, int frameCount = 1) => _layers.First(layer => layer.name == layerName).layerVariations[variation].Height() / frameCount;

    /// <inheritdoc cref="GetLayerFrameHeight(string,int,int)" />
    public int GetLayerFrameHeight(int layerIndex = 0, int variation = 0, int frameCount = 1) => _layers[layerIndex].layerVariations[variation].Height() / frameCount;

    /// <summary>
    ///     Returns the amount of variations that a given layer has.
    /// </summary>
    public int GetLayerVariations(string layerName) => _layers.First(layer => layer.name == layerName).layerVariations.Length;

    /// <inheritdoc cref="GetLayerVariations(string)" />
    public int GetLayerVariations(int layerIndex) => _layers[layerIndex].layerVariations.Length;
}