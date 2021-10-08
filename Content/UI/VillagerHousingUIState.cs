using LivingWorldMod.Content.UI.Elements;
using LivingWorldMod.Custom.Enums;
using Terraria.UI;

namespace LivingWorldMod.Content.UI {

    /// <summary>
    /// UIState that handles the visuals of the Housing UI.
    /// </summary>
    public class VillagerHousingUIState : UIState {

        /// <summary>
        /// Whether or not the menu that shows each of the villagers is visible (open).
        /// </summary>
        public bool isMenuVisible;

        /// <summary>
        /// The Villager type to currently be showing to the player.
        /// </summary>
        public VillagerType typeToShow;

        /// <summary>
        /// The button that closes/opens the menu showing each of the villagers.
        /// </summary>
        public UIBetterImageButton openMenuButton;

        public override void OnInitialize() {
            typeToShow = VillagerType.Harpy;
        }
    }
}