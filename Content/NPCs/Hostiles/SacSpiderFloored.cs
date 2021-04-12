using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.NPCs.Hostiles {

    public class SacSpiderFloored : ModNPC {
        private int frame = 0;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Baby Creeper");
            Main.npcFrameCount[npc.type] = 4;
        }

        public override void SetDefaults() {
            //All stats are half of the Wall Creeper
            npc.CloneDefaults(NPCID.WallCreeper);
            npc.life = 40;
            npc.damage = 15;
            npc.defense = 5;
            npc.aiStyle = -1;
            npc.width = 12;
            npc.height = 8;
            npc.value = Item.sellPrice(copper: 50);
        }

        public override void AI() {
            npc.TargetClosest();
            Player target = Main.player[npc.target];

            float jumpVelocity = 8f;

            if (Collision.CanHitLine(npc.position, 8, 4, target.position, target.width, target.height)) {
                npc.TargetClosest();
                npc.velocity.X = 2f * npc.direction;
                if (npc.collideY && IsAbove(npc.Center, target.Center) && Math.Abs(npc.Center.X - target.Center.X) >= 16 * 5) {
                    npc.velocity.Y -= jumpVelocity;
                }
            }
            else {
                npc.velocity.X = 2f * npc.direction;
                if (npc.collideX && npc.velocity.Y == 0f) {
                    npc.velocity.Y -= jumpVelocity;
                    if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.Next(5) == 0) {
                        npc.direction *= -1;
                        npc.netUpdate = true;
                    }
                }
            }

            npc.spriteDirection = npc.direction;

            if (Main.netMode != NetmodeID.MultiplayerClient && npc.velocity.Y == 0f) {
                int tileCoordX = (int)npc.Center.X / 16;
                int tileCoordY = (int)npc.Center.Y / 16;
                bool isEligibleWall = false;
                for (int i = tileCoordX - 1; i <= tileCoordX + 1; i++) {
                    for (int j = tileCoordY - 1; j <= tileCoordY + 1; j++) {
                        if (Main.tile[i, j].wall > 0) {
                            isEligibleWall = true;
                        }
                    }
                }
                if (isEligibleWall) {
                    npc.Transform(ModContent.NPCType<SacSpiderWalled>());
                }
            }
        }

        public override void FindFrame(int frameHeight) {
            if (npc.velocity.X != 0f) {
                if (++npc.frameCounter > 3) {
                    if (++frame > 3)
                        frame = 0;
                    npc.frame.Y = frame * frameHeight;
                    npc.frameCounter = 0;
                }
            }
            //It would look weird if the spider legs continued to walk even when in contact with a wall, which is thus why this is here
            else {
                npc.frame.Y = frame * frameHeight;
                npc.frameCounter = 0;
            }
        }

        /// <summary>
        /// Return whether or not a given position is above another position. In this case, it
        /// checks if possiblePos is above referencePos
        /// </summary>
        /// <param name="referencePos">
        /// The position used as reference for if the second parameter, possiblePos, is above
        /// </param>
        /// <param name="possiblePos"> Position checked as if it is above referencePos </param>
        /// <returns> </returns>
        private bool IsAbove(Vector2 referencePos, Vector2 possiblePos) => (referencePos.Y - possiblePos.Y) > 32;
    }
}