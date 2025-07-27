using LivingWorldMod.Content.Villages.UI.VillagerHousing;
using LivingWorldMod.Globals.BaseTypes.Systems;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Villages.Globals.Systems.UI;

/// <summary>
///     System that handles the initialization and opening/closing of the Villager Housing UI.
/// </summary>
public class VillagerHousingUISystem : UISystem<VillagerHousingUISystem, VillagerHousingUIState> {
    public override string VanillaInterfaceLocation => "Vanilla: Inventory";

    public override string InternalInterfaceName => "Villager Housing";

    public override void UpdateUI(GameTime gameTime) {
        //Only have the state be changed when the inventory is open, to prevent accidental clicking even if the element is invisible.
        if (Main.playerInventory && !UIIsActive) {
            OpenUIState();
        }
        else if (!Main.playerInventory && !UIIsActive) {
            //We manually set isMenuVisible false here because when the state is null, the value is not updated
            UIState.CloseMenu();

            CloseUIState();
        }

        if (UIIsActive) {
            base.UpdateUI(gameTime);
        }
    }
}