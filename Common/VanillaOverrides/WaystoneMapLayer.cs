using LivingWorldMod.Common.Systems;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Common.VanillaOverrides {

    /// <summary>
    /// IMapLayer class implementation for drawing waystone icons on the map.
    /// </summary>
    public class WaystoneMapLayer : IMapLayer {
        public void Draw(ref MapOverlayDrawContext context, ref string text) {
            WaystoneSystem waystoneSystem = ModContent.GetInstance<WaystoneSystem>();

            for (int i = 0; i < waystoneSystem.waystoneData.Count; i++) {
                WaystoneInfo info = waystoneSystem.waystoneData[i];

                if (!info.isActivated) {
                    continue;
                }

                Texture2D mapIcon = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}MapIcons/Waystones/WaystoneIcon_{info.waystoneType}").Value;
                if (context.Draw(mapIcon, info.iconLocation,
                        TeleportPylonsSystem.IsPlayerNearAPylon(Main.LocalPlayer) ? Color.White : new Color(169, 169, 169, 165),
                        new SpriteFrame(1, 1),
                        1f,
                        1.75f,
                        Alignment.Center)
                    .IsMouseOver) {
                    text = "Waystone";
                }
            }
        }
    }
}
