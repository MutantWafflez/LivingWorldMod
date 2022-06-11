using System.Linq;
using LivingWorldMod.Content.TileEntities.Interactables.VillageShrines;
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
                if (!Main.BestiaryTracker.Chats.GetWasChatWith($"{nameof(LivingWorldMod)}/{entity.shrineType}Villager")) {
                    continue;
                }

                Texture2D mapIcon = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}MapIcons/VillageShrines/ShrineIcon_{entity.shrineType}").Value;
                Color drawColor = TeleportPylonsSystem.IsPlayerNearAPylon(Main.LocalPlayer) ? Color.White : Color.Gray * 0.5f;
                if (!context.Draw(mapIcon, entity.Position.ToVector2() + new Vector2(2f, 2.5f), drawColor, new SpriteFrame(1, 1), 1f, 1.75f, Alignment.Center).IsMouseOver) {
                    continue;
                }

                if (!Main.mouseLeft || !Main.mouseLeftRelease) {
                    continue;
                }
                //Close map, undo mouse click
                Main.mouseLeftRelease = false;
                Main.mapFullscreen = false;

                //Create fake pylon data to make the game think this shrine is a universal pylon
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