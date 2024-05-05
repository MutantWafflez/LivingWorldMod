using System.Linq;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.Villages.HarpyVillage.NPCs;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.UI;

namespace LivingWorldMod.Content.Villages.UI.VillagerShop;

/// <summary>
/// UIElement class extension that handles and creates portraits for villagers in the shop UI, primarily.
/// </summary>
public class UIPortrait : UIElement {
    /// <summary>
    /// Small enum that defines what expression fits with what sprite on the portraits of the shop
    /// UI.
    /// </summary>
    public enum VillagerPortraitExpression {
        Neutral,
        Happy,
        Angered
    }

    private string PortraitSpritePath => $"{LWM.SpritePath}Villages/UI/ShopUI/{_villager.VillagerType}/Portraits/";

    public VillagerPortraitExpression temporaryExpression;
    public float temporaryExpressionTimer;

    // TODO: Make array when more villages are added
    private readonly LayeredDrawObject _drawObject;
    private int[] _portraitDrawIndices;
    private VillagerPortraitExpression _currentExpression;
    private Villager _villager;

    public UIPortrait(Villager villager) {
        _villager = villager;
        Width.Set(190f, 0f);
        Height.Set(190f, 0f);

        _drawObject = new LayeredDrawObject([("Base", 5), ("Outfit", 5), ("Hair", 5), ("Face", 15)], PortraitSpritePath);
    }

    public override void OnInitialize() {
        OnLeftClick += ClickedElement;
    }

    public override void Update(GameTime gameTime) {
        //Allows for temporary expressions, for whatever reason that it may be applicable
        if (--temporaryExpressionTimer <= 0f) {
            temporaryExpressionTimer = -1f;
        }

        base.Update(gameTime);
    }

    public void ReloadPortrait(Villager newVillager) {
        _villager = newVillager;

        switch (_villager.RelationshipStatus) {
            case <= VillagerRelationship.SevereDislike:
                _currentExpression = VillagerPortraitExpression.Angered;
                break;

            case > VillagerRelationship.SevereDislike and < VillagerRelationship.Love:
                _currentExpression = VillagerPortraitExpression.Neutral;
                break;

            case >= VillagerRelationship.Love:
                _currentExpression = VillagerPortraitExpression.Happy;
                break;
        }

        const int paleSkinFrame = 0;
        const int tanSkinFrame = 1;
        const int darkSkinFrame = 2;

        // The index for the tan skin color in terms of sprite file names
        // Here for readability
        const int tanSkinIndex = 2;

        int[] villagerDrawIndices = _villager.DrawIndices;
        int faceSkinFrame = villagerDrawIndices[HarpyVillager.BodyIndexID] switch {
            < tanSkinIndex => paleSkinFrame,
            tanSkinIndex => tanSkinFrame,
            > tanSkinIndex => darkSkinFrame
        };

        _portraitDrawIndices = [
            villagerDrawIndices[HarpyVillager.BodyIndexID],
            villagerDrawIndices[HarpyVillager.OutfitIndexID],
            villagerDrawIndices[HarpyVillager.HairIndexID],
            villagerDrawIndices[HarpyVillager.FaceIndexID] * 3 + faceSkinFrame
        ];
    }


    protected override void DrawSelf(SpriteBatch spriteBatch) {
        int frameWidth = _drawObject.GetLayerFrameWidth();
        int frameHeight = _drawObject.GetLayerFrameHeight();

        Rectangle faceRect = new(0, (int)(temporaryExpressionTimer > 0 ? temporaryExpression : _currentExpression) * frameHeight, frameWidth, frameHeight);
        DrawData defaultDrawData = new(
            null,
            GetDimensions().ToRectangle(),
            null,
            Color.White,
            0f,
            default(Vector2),
            SpriteEffects.None
        );

        _drawObject.Draw(
            spriteBatch,
            Enumerable.Repeat(defaultDrawData, _portraitDrawIndices.Length - 1).Append(defaultDrawData with { sourceRect = faceRect }).ToArray(),
            _portraitDrawIndices
        );
    }

    private void ClickedElement(UIMouseEvent evt, UIElement listeningElement) {
        //Little Easter Egg where clicking on the Portrait will make them smile for a half a second
        temporaryExpression = VillagerPortraitExpression.Happy;
        temporaryExpressionTimer = 30f;
        SoundEngine.PlaySound(SoundID.Item16);
    }
}