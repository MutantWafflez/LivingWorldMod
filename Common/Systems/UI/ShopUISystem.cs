using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Content.UI;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Common.Systems.UI {

    /// <summary>
    /// System that handles the initialization and opening/closing of the Shop UI for Villagers.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class ShopUISystem : ModSystem {
        public static Asset<Effect> hoverFlashShader;
        public static Asset<Effect> grayScaleShader;

        public UserInterface shopInterface;
        public ShopUIState shopState;
        public GameTime lastGameTime;

        public override void Load() {
            shopInterface = new UserInterface();
            shopState = new ShopUIState();

            shopState.Activate();

            hoverFlashShader = Mod.Assets.Request<Effect>("Assets/Shaders/UI/ShopItemHoverFlash");
            grayScaleShader = Mod.Assets.Request<Effect>("Assets/Shaders/UI/Grayscale");
        }

        public override void Unload() {
            shopInterface = null;
            shopState = null;

            hoverFlashShader = null;
            grayScaleShader = null;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "LWM: Shop Interface",
                    delegate {
                        if (lastGameTime != null && shopInterface?.CurrentState != null) {
                            shopInterface.Draw(Main.spriteBatch, lastGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        public override void UpdateUI(GameTime gameTime) {
            lastGameTime = gameTime;
            if (shopInterface?.CurrentState != null) {
                shopInterface.Update(gameTime);
            }
        }

        public override void PostUpdateTime() {
            if (Main.time >= 32400.0 && !Main.dayTime && (!Main.gameMenu || Main.netMode == NetmodeID.Server)) {
                NPCUtilities.DoActionForEachNPC(npc => {
                    if (npc.active && npc.IsTypeOfVillager(out Villager villager)) {
                        villager.RestockShop();
                    }
                });
            }
        }

        public override void PostUpdateEverything() {
            //Close shop UI when the player stops talking to the villager or starts talking to a non-villager
            int talkNPC = Main.LocalPlayer.talkNPC;
            if (shopInterface.CurrentState != null && (talkNPC == -1 || (!Main.npc[talkNPC].ModNPC?.GetType().IsSubclassOf(typeof(Villager)) ?? true))) {
                CloseShopUI();
            }
        }

        /// <summary>
        /// Reloads and open the shop UI depending on the villager type being spoken to.
        /// </summary>
        /// <param name="villager"> </param>
        public void OpenShopUI(Villager villager) {
            Main.npcChatText = "";
            shopState.ReloadUI(villager);
            shopState.SetSelectedItem(null, false);
            shopInterface.SetState(shopState);
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        /// <summary>
        /// Closes the shop UI. That is all for now.
        /// </summary>
        public void CloseShopUI() {
            shopState.SetSelectedItem(null, false);
            shopInterface.SetState(null);
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
    }
}