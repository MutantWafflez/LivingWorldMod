using LivingWorldMod.Common.Systems;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.MapLayers {
    /// <summary>
    /// IMapLayer class implementation for drawing waystone icons on the map.
    /// </summary>
    public class WaystoneMapLayer : ModMapLayer {
        public override void Draw(ref MapOverlayDrawContext context, ref string text) {
            WaystoneSystem waystoneSystem = ModContent.GetInstance<WaystoneSystem>();

            for (int i = 0; i < waystoneSystem.waystoneData.Count; i++) {
                WaystoneInfo info = waystoneSystem.waystoneData[i];

                if (!info.isActivated) {
                    continue;
                }

                Texture2D mapIcon = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}MapIcons/Waystones/WaystoneIcon_{info.waystoneType}").Value;
                Color drawColor = TeleportPylonsSystem.IsPlayerNearAPylon(Main.LocalPlayer) ? Color.White : Color.Gray * 0.5f;
                if (!context.Draw(mapIcon, info.iconLocation, drawColor, new SpriteFrame(1, 1), 1f, 1.75f, Alignment.Center).IsMouseOver) {
                    continue;
                }

                text = LocalizationUtils.GetLWMTextValue($"MapInfo.Waystones.{info.waystoneType}");

                if (!Main.mouseLeft || !Main.mouseLeftRelease) {
                    continue;
                }
                //Close map, undo mouse click
                Main.mouseLeftRelease = false;
                Main.mapFullscreen = false;

                //Create fake pylon data to make the game think this waystone is a universal pylon
                TeleportPylonInfo fakePylonInfo = default;
                fakePylonInfo.PositionInTiles = info.tileLocation;
                fakePylonInfo.TypeOfPylon = TeleportPylonType.Victory;

                //Request teleportation
                Main.PylonSystem.RequestTeleportation(fakePylonInfo, Main.LocalPlayer);
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }
    }
}