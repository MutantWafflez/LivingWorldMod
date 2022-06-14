using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Content.UI.VillageShrine;
using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace LivingWorldMod.Common.Systems.UI {
    public class VillageShrineUISystem : UISystem<VillageShrineUIState> {
        public override string VanillaInterfaceLocation => "Vanilla: Inventory";

        public override string InternalInterfaceName => "Village Shrine Panel";

        public override InterfaceScaleType ScaleType => InterfaceScaleType.None;

        /// <summary>
        /// Opens (if no shrine panel is already open) or regens the ShrineUIState (if a shrine panel is currently open)
        /// with the passed in parameters. 
        /// </summary>
        /// <param name="newShrineType"> The new shrine type to swap the state to. </param>
        /// <param name="topLeftShrinePos"> The WORLD position of where to move the state to. </param>
        public void OpenOrRegenShrineState(VillagerType newShrineType, Vector2 topLeftShrinePos) {
            if (correspondingInterface.CurrentState is null) {
                correspondingInterface.SetState(correspondingUIState);
            }

            if (correspondingInterface.CurrentState == correspondingUIState) {
                correspondingUIState.RegenState(newShrineType, topLeftShrinePos);
            }
        }

        /// <summary>
        /// Simply closes the ShrineUIState by setting the corresponding interface's state to null.
        /// </summary>
        public void CloseShrineState() {
            correspondingInterface.SetState(null);
        }
    }
}