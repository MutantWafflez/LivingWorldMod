using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Common.Systems.SubworldSystems {
    /// <summary>
    /// ModSystem that handles itself in the capacity of the Pyramid Dungeon
    /// Subworld ONLY.
    /// </summary>
    public class PyramidDungeonSystem : BaseModSystem<PyramidDungeonSystem> {
        public class TorchCountdown {
            public Point torchPos;
            public int deathCountdown;

            public TorchCountdown(Point torchPos, int deathCountdown) {
                this.torchPos = torchPos;
                this.deathCountdown = deathCountdown;
            }
        }

        private readonly List<TorchCountdown> _torchCountdowns = new();

        public override void PostDrawTiles() {
            if (!PyramidSubworld.IsInSubworld) {
                return;
            }

            Rectangle screenRect = new((int)Main.screenPosition.X - 16, (int)Main.screenPosition.Y - 16, Main.screenWidth + 16, Main.screenHeight + 16);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            for (int i = 0; i < Main.maxTilesX; i++) {
                for (int j = 0; j < Main.maxTilesY; j++) {
                    if (!screenRect.Contains(i * 16, j * 16)) {
                        continue;
                    }

                    if (!(Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>().currentRoom?.region.Contains(i, j) ?? true)) {
                        Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i, j).ToWorldCoordinates(Vector2.Zero) - Main.screenPosition, Color.Black);
                    }
                }
            }
            Main.spriteBatch.End();
        }

        public override void PostUpdateEverything() {
            if (!PyramidSubworld.IsInSubworld) {
                return;
            }

            ModContent.GetInstance<PyramidSubworld>().grid.UpdateAllRooms();

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                return;
            }

            //Do countdown for each current torch
            for (int i = 0; i < _torchCountdowns.Count; i++) {
                TorchCountdown countdown = _torchCountdowns[i];

                if (--countdown.deathCountdown > 0) {
                    continue;
                }

                //Double check that tile is indeed a torch, and remove torch
                Tile tile = Main.tile[countdown.torchPos];
                if (tile.TileType == TileID.Torches) {
                    tile.TileType = 0;
                    tile.HasTile = false;
                    NetMessage.SendTileSquare(-1, countdown.torchPos.X, countdown.torchPos.Y);
                }

                _torchCountdowns.Remove(countdown);
                i--;
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            if (!(Main.LocalPlayer.GetModPlayer<PyramidDungeonPlayer>().currentRoom?.ActiveCurses.Contains(PyramidRoomCurseType.Insensitivity) ?? false)) {
                return;
            }

            GameInterfaceLayer healthLayer = layers.FirstOrDefault(layer => layer.Name == "Vanilla: Resource Bars");

            if (healthLayer is not null) {
                healthLayer.Active = false;
            }
        }

        /// <summary>
        /// Adds a new torch being marked for death at the specified position.
        /// </summary>
        public void AddNewDyingTorch(Point torchPos) {
            if (_torchCountdowns.All(countdown => countdown.torchPos != torchPos)) {
                _torchCountdowns.Add(new TorchCountdown(torchPos, 20 * 60));
            }
        }

        /// <summary>
        /// Removes all currently active torches marked for death.
        /// </summary>
        public void PurgeTorchList() {
            _torchCountdowns.Clear();
        }
    }
}