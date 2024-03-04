using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Furniture;
using LivingWorldMod.Content.Villages.UI.VillageShrine;
using LivingWorldMod.Custom.Utilities;
using LivingWorldMod.Globals.Systems.BaseSystems;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.UI;

namespace LivingWorldMod.Content.Villages.Globals.Systems.UI;

public class VillageShrineUISystem : UISystem<VillageShrineUIState> {
    public override string VanillaInterfaceLocation => "Vanilla: Inventory";

    public override string InternalInterfaceName => "Village Shrine Panel";

    public override InterfaceScaleType ScaleType => InterfaceScaleType.Game;

    public override void PostUpdateEverything() {
        if (correspondingInterface.CurrentState == correspondingUIState && correspondingUIState.ShowVillageRadius && correspondingUIState.CurrentEntity is { } entity) {
            Dust dust = Dust.NewDustPerfect(entity.Position.ToWorldCoordinates(32f, 40f), DustID.BlueFairy);
            dust.active = false;
            dust.noGravity = true;
            dust.scale = 1.25f;

            LWMUtils.CreateCircle(dust.position, VillageShrineEntity.DefaultVillageRadius, dust);
        }
    }

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
    /// <param name="entityPos"> The position of the new entity to bind to. </param>
    public void OpenOrRegenShrineState(Point16 entityPos) {
        if (correspondingInterface.CurrentState is null) {
            correspondingInterface.SetState(correspondingUIState);
        }

        if (correspondingInterface.CurrentState == correspondingUIState) {
            correspondingUIState.RegenState(entityPos);
        }
    }

    /// <summary>
    /// Simply closes the ShrineUIState by setting the corresponding interface's state to null.
    /// </summary>
    public void CloseShrineState() {
        correspondingInterface.SetState(null);
    }
}