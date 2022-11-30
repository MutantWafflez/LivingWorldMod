﻿using System.Collections.Generic;
using LivingWorldMod.Common.Players;
using Terraria;
using LivingWorldMod.Common.Systems.BaseSystems;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;

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

        public List<TorchCountdown> torchCountdowns = new List<TorchCountdown>();

        public bool IsInPyramidSubworld => SubworldSystem.IsActive<PyramidSubworld>();

        public override void PostDrawTiles() {
            if (!IsInPyramidSubworld) {
                return;
            }

            Rectangle screenRect = new Rectangle((int)Main.screenPosition.X - 16, (int)Main.screenPosition.Y - 16, Main.screenWidth + 16, Main.screenHeight + 16);

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
            if (!IsInPyramidSubworld || Main.netMode == NetmodeID.MultiplayerClient) {
                return;
            }

            //Do countdown for each current torch
            for (int i = 0; i < torchCountdowns.Count; i++) {
                TorchCountdown countdown = torchCountdowns[i];

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

                torchCountdowns.Remove(countdown);
                i--;
            }
        }

        public void AddNewDyingTorch(Point torchPos) {
            torchCountdowns.Add(new TorchCountdown(torchPos, 20 * 60));
        }
    }
}