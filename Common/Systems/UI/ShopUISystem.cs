﻿using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Content.UI.VillagerShop;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace LivingWorldMod.Common.Systems.UI {
    /// <summary>
    /// System that handles the initialization and opening/closing of the Shop UI for Villagers.
    /// </summary>
    public class ShopUISystem : UISystem<ShopUIState> {
        public static Asset<Effect> hoverFlashShader;
        public static Asset<Effect> grayScaleShader;

        public override string InternalInterfaceName => "Villager Shop";

        public override void Load() {
            base.Load();

            hoverFlashShader = Mod.Assets.Request<Effect>("Assets/Shaders/UI/ShopItemHoverFlash");
            grayScaleShader = Mod.Assets.Request<Effect>("Assets/Shaders/UI/Grayscale");
        }

        public override void Unload() {
            hoverFlashShader = null;
            grayScaleShader = null;
        }

        public override void PostUpdateTime() {
            if (Main.time >= 32400.0 && !Main.dayTime && (!Main.gameMenu || Main.netMode == NetmodeID.Server)) {
                foreach (NPC npc in Main.npc) {
                    if (npc.active && npc.ModNPC is Villager villager) {
                        villager.RestockShop();
                    }
                }
            }
        }

        public override void PostUpdateEverything() {
            //Close shop UI when the player stops talking to the villager or starts talking to a non-villager
            int talkNPC = Main.LocalPlayer.talkNPC;
            if (correspondingInterface.CurrentState != null && (talkNPC == -1 || (!Main.npc[talkNPC].ModNPC?.GetType().IsSubclassOf(typeof(Villager)) ?? true))) {
                CloseShopUI();
            }
        }

        /// <summary>
        /// Reloads and open the shop UI depending on the villager type being spoken to.
        /// </summary>
        /// <param name="villager"> </param>
        public void OpenShopUI(Villager villager) {
            Main.npcChatText = "";
            correspondingUIState.ReloadUI(villager);
            correspondingUIState.SetSelectedItem(null, false);
            correspondingInterface.SetState(correspondingUIState);
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        /// <summary>
        /// Closes the shop UI. That is all for now.
        /// </summary>
        public void CloseShopUI() {
            correspondingUIState.SetSelectedItem(null, false);
            correspondingInterface.SetState(null);
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
    }
}