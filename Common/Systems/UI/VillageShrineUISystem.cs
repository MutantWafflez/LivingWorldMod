using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Content.UI.VillageShrine;
using Terraria.UI;

namespace LivingWorldMod.Common.Systems.UI {
    public class VillageShrineUISystem : UISystem<VillageShrineUIState> {
        public override string VanillaInterfaceLocation => "Vanilla: Inventory";

        public override string InternalInterfaceName => "Village Shrine Panel";

        public override InterfaceScaleType ScaleType => InterfaceScaleType.None;
    }
}