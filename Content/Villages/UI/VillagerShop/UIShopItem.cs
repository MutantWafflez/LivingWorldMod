using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.DataStructures.Records;
using LivingWorldMod.Content.Villages.Globals.Systems.UI;
using LivingWorldMod.Globals.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.Villages.UI.VillagerShop;

/// <summary>
///     UIImage class that holds different UITexts and UIElements that is the index in a given shop
///     UI list. Holds data on the entire entry for the given item.
/// </summary>
public class UIShopItem : UIImage {
    public UIBetterItemIcon itemImage;
    public UIBetterText itemNameText;
    public UICoinDisplay itemCostDisplay;

    /// <summary>
    ///     The ShopItem class object that this element is tied to. Is used to sync the villager's
    ///     inventory with the UI properly.
    /// </summary>
    public readonly ShopItem pertainedInventoryItem;

    public readonly Item displayedItem;

    public VillagerType villagerType;

    public long displayedCost;

    public bool isSelected;

    private float _manualUpdateTime;

    public UIShopItem(ShopItem pertainedInventoryItem, long displayedCost, VillagerType villagerType) : base(
        ModContent.Request<Texture2D>($"{LWM.SpritePath}Villages/UI/ShopUI/{villagerType}/ShopItemBox")
    ) {
        this.pertainedInventoryItem = pertainedInventoryItem;
        displayedItem = new Item();
        displayedItem.SetDefaults(pertainedInventoryItem.itemType);
        displayedItem.stack = pertainedInventoryItem.remainingStock;
        this.displayedCost = displayedCost;
        this.villagerType = villagerType;
    }

    public override void OnInitialize() {
        float itemImageSize = 32f;

        itemImage = new UIBetterItemIcon(displayedItem, itemImageSize, true) { VAlign = 0.5f };
        itemImage.Left.Set(38f, 0f);
        itemImage.Width.Set(itemImageSize, 0f);
        itemImage.Height.Set(itemImageSize, 0f);
        Append(itemImage);

        itemNameText = new UIBetterText(displayedItem.HoverName, 1.25f) { VAlign = 0.5f, horizontalTextConstraint = 194f };
        itemNameText.Left.Set(94f, 0f);
        Append(itemNameText);

        itemCostDisplay = new UICoinDisplay(displayedCost, UICoinDisplay.CoinDrawStyle.NoCoinsWithZeroValue, 1.34f) { VAlign = 0.5f };
        itemCostDisplay.Left.Set(-itemCostDisplay.Width.Pixels - 12f, 1f);
        Append(itemCostDisplay);
    }

    public override void MouseOver(UIMouseEvent evt) {
        base.MouseOver(evt);

        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void MouseOut(UIMouseEvent evt) {
        base.MouseOut(evt);

        if (!isSelected) {
            _manualUpdateTime = 0f;
        }
    }

    public override void LeftClick(UIMouseEvent evt) {
        ShopUIState shopState = ShopUISystem.Instance.correspondingUIState;

        shopState.SetSelectedItem(!isSelected && pertainedInventoryItem.remainingStock > 0 ? this : null);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        RasterizerState defaultRasterizerState = new() { CullMode = CullMode.None, ScissorTestEnable = true };

        if (pertainedInventoryItem.remainingStock <= 0) {
            Effect shader = ShopUISystem.grayScaleShader.Value;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, defaultRasterizerState, shader, Main.UIScaleMatrix);

            base.DrawSelf(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, defaultRasterizerState, null, Main.UIScaleMatrix);
            return;
        }

        if (ContainsPoint(Main.MouseScreen) || isSelected) {
            Effect shader = ShopUISystem.hoverFlashShader.Value;

            _manualUpdateTime += 1f / 45f;
            if (_manualUpdateTime >= MathHelper.TwoPi) {
                _manualUpdateTime = 0f;
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, defaultRasterizerState, shader, Main.UIScaleMatrix);

            //So I am unsure as to why exactly this needed to be done, cause this is definitely the definition of a band-aid fix.
            //In short, when using this shader, uTime isn't being updated at all, causing the shader to just stay one color instead of breathing in a sine wave fashion like intended.
            //Thus, for the time being, until I can figure out why uTime isn't being automatically updated, I am manually setting this new Parameter
            shader.Parameters["manualUTime"].SetValue(_manualUpdateTime);
            base.DrawSelf(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, defaultRasterizerState, null, Main.UIScaleMatrix);
        }
        else {
            base.DrawSelf(spriteBatch);
        }
    }
}