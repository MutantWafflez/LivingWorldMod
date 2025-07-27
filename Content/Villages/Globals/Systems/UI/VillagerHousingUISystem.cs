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
        UIState.SetMenuButtonVisibility(Main.playerInventory);

        base.UpdateUI(gameTime);
    }

    
    // TODO: Fix this. TL;DR the issue is that Main.UIScale isn't properly set until Player.Hooks.OnEnterWorld is called, but waiting this long triggers errors in UpdateUI above ^
    protected override void InitializeUIState(Player player) {
        base.InitializeUIState(player);

        OpenUIState();
    }
}