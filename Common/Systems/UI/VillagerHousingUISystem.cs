using LivingWorldMod.Content.UI.VillagerHousing;
using Microsoft.Xna.Framework;
using Terraria;

namespace LivingWorldMod.Common.Systems.UI {
    /// <summary>
    /// System that handles the initialization and opening/closing of the Villager Housing UI.
    /// </summary>
    public class VillagerHousingUISystem : UISystem<VillagerHousingUIState> {
        public override string VanillaInterfaceLocation => "Vanilla: Inventory";

        public override string InternalInterfaceName => "Villager Housing";

        public override void UpdateUI(GameTime gameTime) {
            lastGameTime = gameTime;
            //Only have the state be changed when the inventory is open, to prevent accidental clicking even if the element is invisible.
            if (Main.playerInventory && correspondingInterface.CurrentState is null) {
                correspondingInterface.SetState(correspondingUIState);
            }
            else if (!Main.playerInventory && correspondingInterface?.CurrentState is not null) {
                //We manually set isMenuVisible false here because when the state is null, the value is not updated
                correspondingUIState.CloseMenu();

                correspondingInterface.SetState(null);
            }
            if (correspondingInterface?.CurrentState is not null) {
                correspondingInterface.Update(gameTime);
            }
        }
    }
}