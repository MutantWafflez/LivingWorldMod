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

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        // One of the few UIs that need to be loaded immediately
        OpenUIState();
    }

    public override void UpdateUI(GameTime gameTime) {
        // Only enable open button when inventory is open
        UIState.openMenuButtonZone.SetVisibility(Main.playerInventory);

        base.UpdateUI(gameTime);
    }
}