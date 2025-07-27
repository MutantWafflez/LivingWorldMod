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
        // Only enable open button when inventory is open
        UIState?.SetMenuButtonVisibility(Main.playerInventory);

        base.UpdateUI(gameTime);
    }

    protected override void InitializeUIState(Player player) {
        base.InitializeUIState(player);

        // This is one of the few UIs that is "open" indefinitely
        OpenUIState();
    }
}