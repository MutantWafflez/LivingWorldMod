using System.Linq;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.TileEntities.Interactables.VillageShrines;
using LivingWorldMod.Custom.Enums;
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
    /// MapLayer that acts extremely similarly to vanilla's pylon map layer, but for the
    /// village shrines.
    /// </summary>
    public class VillageShrineMapLayer : ModMapLayer {
        public override void Draw(ref MapOverlayDrawContext context, ref string text) {
            foreach (VillageShrineEntity entity in TileEntity.ByID.Values.OfType<VillageShrineEntity>()) {
                if (!IsShrineVisibleOnMap(entity.shrineType)) {
                    continue;
                }

                Texture2D mapIcon = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}MapIcons/VillageShrines/ShrineIcon_{entity.shrineType}").Value;
                Color drawColor = TeleportPylonsSystem.IsPlayerNearAPylon(Main.LocalPlayer) ? Color.White : Color.Gray * 0.5f;
                if (!context.Draw(mapIcon, entity.Position.ToVector2() + new Vector2(2f, 2.5f), drawColor, new SpriteFrame(1, 1), 1f, 1.75f, Alignment.Center).IsMouseOver) {
                    continue;
                }

                text = LocalizationUtils.GetLWMTextValue($"MapInfo.Shrines.{entity.shrineType}");

                if (!Main.mouseLeft || !Main.mouseLeftRelease) {
                    continue;
                }
                //Close map, undo mouse click
                Main.mouseLeftRelease = false;
                Main.mapFullscreen = false;

                //Create fake pylon data to make the game think this shrine is a universal pylon
                TeleportPylonInfo fakePylonInfo = default;
                fakePylonInfo.PositionInTiles = entity.Position + new Point16(1, 1);
                fakePylonInfo.TypeOfPylon = TeleportPylonType.Victory;

                //Request teleportation
                Main.PylonSystem.RequestTeleportation(fakePylonInfo, Main.LocalPlayer);
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }

        /// <summary>
        /// Returns whether or not the given village type will have their shrine icon visible at all on the map.
        /// </summary>
        /// <param name="type"> The type of the village who's shrine we are referring to. </param>
        private bool IsShrineVisibleOnMap(VillagerType type) {
            switch (type) {
                case VillagerType.Harpy:
                    return Main.BestiaryTracker.Chats.GetWasChatWith($"{nameof(LivingWorldMod)}/HarpyVillager");
                default:
                    ModContent.GetInstance<LivingWorldMod>().Logger.Error($"Villager Type of {type} is not valid for shrine visibility.");
                    return false;
            }
        }
    }
}