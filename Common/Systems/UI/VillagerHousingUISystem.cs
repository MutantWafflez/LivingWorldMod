using System.Collections.Generic;
using LivingWorldMod.Content.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Common.Systems.UI {

    /// <summary>
    /// System that handles the initialization and opening/closing of the Villager Housing UI.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class VillagerHousingUISystem : ModSystem {
        public UserInterface housingInterface;
        public VillagerHousingUIState housingState;
        public GameTime lastGameTime;

        public override void Load() {
            housingInterface = new UserInterface();
            housingState = new VillagerHousingUIState();

            housingState.Activate();

            housingInterface.SetState(housingState);
        }

        public override void Unload() {
            housingInterface = null;
            housingState = null;

            lastGameTime = null;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1) {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "LWM: Villager Housing Interface",
                    delegate {
                        if (lastGameTime != null && housingInterface?.CurrentState != null) {
                            housingInterface.Draw(Main.spriteBatch, lastGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        public override void UpdateUI(GameTime gameTime) {
            lastGameTime = gameTime;
            if (housingInterface?.CurrentState != null) {
                housingInterface.Update(gameTime);
            }
        }
    }
}