using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Content.UI.VillageShrine;
using Microsoft.Xna.Framework;
using Terraria.GameInput;
using Terraria.UI;

namespace LivingWorldMod.Common.Systems.UI {
    public class VillageShrineUISystem : UISystem<VillageShrineUIState> {
        public override string VanillaInterfaceLocation => "Vanilla: Inventory";

        public override string InternalInterfaceName => "Village Shrine Panel";

        public override InterfaceScaleType ScaleType => InterfaceScaleType.Game;

        public override void UpdateUI(GameTime gameTime) {
            //Although the zoom is properly adjusted for drawing, it is NOT properly adjusted for updating, and since we're using
            //the "Game" scaling type, we need to set the zoom to the world for hovering/clicking to work properly.
            PlayerInput.SetZoom_World();
            base.UpdateUI(gameTime);
            PlayerInput.SetZoom_UI();
        }

        /// <summary>
        /// Opens (if no shrine panel is already open) or regens the ShrineUIState (if a shrine panel is currently open)
        /// with the passed in entity.
        /// </summary>
        /// <param name="entity"></param>
        public void OpenOrRegenShrineState(VillageShrineEntity entity) {
            if (correspondingInterface.CurrentState is null) {
                correspondingInterface.SetState(correspondingUIState);
            }

            if (correspondingInterface.CurrentState == correspondingUIState) {
                correspondingUIState.RegenState(entity);
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