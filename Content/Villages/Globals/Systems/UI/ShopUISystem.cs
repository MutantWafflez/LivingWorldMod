using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.Villages.UI.VillagerShop;
using LivingWorldMod.Globals.BaseTypes.Systems;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;

namespace LivingWorldMod.Content.Villages.Globals.Systems.UI;

/// <summary>
///     System that handles the initialization and opening/closing of the Shop UI for Villagers.
/// </summary>
public class ShopUISystem : UISystem<ShopUISystem, ShopUIState> {
    public static Asset<Effect> hoverFlashShader;
    public static Asset<Effect> grayScaleShader;

    public override string InternalInterfaceName => "Villager Shop";

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        hoverFlashShader = Mod.Assets.Request<Effect>("Assets/Shaders/UI/ShopItemHoverFlash");
        grayScaleShader = Mod.Assets.Request<Effect>("Assets/Shaders/UI/Grayscale");
    }

    public override void Unload() {
        hoverFlashShader = null;
        grayScaleShader = null;
    }

    public override void PostUpdateTime() {
        if (!(Main.time >= LWMUtils.InGameMoonlight) || Main.dayTime || (Main.gameMenu && Main.netMode != NetmodeID.Server)) {
            return;
        }

        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.ModNPC is not Villager villager) {
                continue;
            }

            villager.RegenerateShop();
        }
    }

    public override void UpdateUI(GameTime gameTime) {
        base.UpdateUI(gameTime);

        //Close shop UI when the player stops talking to the villager or starts talking to a non-villager
        if (!UIIsActive || Main.LocalPlayer.TalkNPC is { ModNPC: Villager }) {
            return;
        }

        CloseShopUI();
    }

    /// <summary>
    ///     Reloads and open the shop UI depending on the villager type being spoken to.
    /// </summary>
    public void OpenShopUI(Villager villager) {
        Main.npcChatText = "";

        UIState.ReloadUI(villager);
        UIState.SetSelectedItem(null, false);

        OpenUIState();

        SoundEngine.PlaySound(SoundID.MenuOpen);
    }

    /// <summary>
    ///     Closes the shop UI. That is all for now.
    /// </summary>
    public void CloseShopUI() {
        UIState.SetSelectedItem(null, false);

        CloseUIState();

        SoundEngine.PlaySound(SoundID.MenuClose);
    }
}