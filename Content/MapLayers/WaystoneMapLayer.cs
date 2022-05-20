using System.Linq;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.TileEntities.Interactables;
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
            foreach (WaystoneEntity entity in TileEntity.ByID.Values.OfType<WaystoneEntity>()) {
                if (!entity.isActivated) {
                    continue;
                }

                Texture2D mapIcon = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}MapIcons/Waystones/WaystoneIcon_{entity.waystoneType}").Value;
                Color drawColor = TeleportPylonsSystem.IsPlayerNearAPylon(Main.LocalPlayer) ? Color.White : Color.Gray * 0.5f;
                if (!context.Draw(mapIcon, entity.Position.ToVector2() + new Vector2(1f, 1.5f), drawColor, new SpriteFrame(1, 1), 1f, 1.75f, Alignment.Center).IsMouseOver) {
                    continue;
                }

                text = LocalizationUtils.GetLWMTextValue($"MapInfo.Waystones.{entity.waystoneType}");

                if (!Main.mouseLeft || !Main.mouseLeftRelease) {
                    continue;
                }
                //Close map, undo mouse click
                Main.mouseLeftRelease = false;
                Main.mapFullscreen = false;

                //Create fake pylon data to make the game think this waystone is a universal pylon
                TeleportPylonInfo fakePylonInfo = default;
                fakePylonInfo.PositionInTiles = entity.Position;
                fakePylonInfo.TypeOfPylon = TeleportPylonType.Victory;

                //Request teleportation
                Main.PylonSystem.RequestTeleportation(fakePylonInfo, Main.LocalPlayer);
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }
    }
}