using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.NPCs.Hostiles
{
    public class SacSpiderWalled : ModNPC
    {
        private int frame = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Baby Creeper");
            Main.npcFrameCount[npc.type] = 4;
        }

        public override void SetDefaults()
        {
            //All stats are half of the Wall Creeper
            npc.CloneDefaults(NPCID.WallCreeperWall);
            npc.life = 40;
            npc.damage = 15;
            npc.defense = 5;
            npc.aiStyle = -1;
            npc.width = 12;
            npc.height = 12;
            npc.value = Item.sellPrice(copper: 50);
        }

        //Adapted Wall Creeper (Walled) AI
        //Can't just use the vanilla one, since the transformation of walled <--> floored is hardcoded, so it must be adapted
        public override void AI()
        {
            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead)
                npc.TargetClosest();

            Player target = Main.player[npc.target];

            float mathVariable = 2f; //Honestly not 100% what this variable does, but it is used so it stays
            float weight = 0.08f;

            Vector2 centerPlaceholder = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
            float npcXCenter = target.position.X + target.width / 2;
            float npcYCenter = target.position.Y + target.height / 2;
            npcXCenter = (int)(npcXCenter / 8f) * 8;
            npcYCenter = (int)(npcYCenter / 8f) * 8;
            centerPlaceholder.X = (int)(centerPlaceholder.X / 8f) * 8;
            centerPlaceholder.Y = (int)(centerPlaceholder.Y / 8f) * 8;
            npcXCenter -= centerPlaceholder.X;
            npcYCenter -= centerPlaceholder.Y;
            if (npc.confused)
            {
                npcXCenter *= -2f;
                npcYCenter *= -2f;
            }

            float vectorLength = (float)Math.Sqrt(npcXCenter * npcXCenter + npcYCenter * npcYCenter);
            if (vectorLength == 0f)
            {
                npcXCenter = npc.velocity.X;
                npcYCenter = npc.velocity.Y;
            }
            else
            {
                vectorLength = mathVariable / vectorLength;
                npcXCenter *= vectorLength;
                npcYCenter *= vectorLength;
            }

            if (target.dead)
            {
                npcXCenter = npc.direction * mathVariable / 2f;
                npcYCenter = (0f - mathVariable) / 2f;
            }

            if (!Collision.CanHit(npc.position, npc.width, npc.height, target.position, target.width, target.height))
            {
                npc.ai[0] += 1f;
                if (npc.ai[0] > 0f)
                    npc.velocity.Y += 0.023f;
                else
                    npc.velocity.Y -= 0.023f;

                if (npc.ai[0] < -100f || npc.ai[0] > 100f)
                    npc.velocity.X += 0.023f;
                else
                    npc.velocity.X -= 0.023f;

                if (npc.ai[0] > 200f)
                    npc.ai[0] = -200f;

                npc.velocity.X += npcXCenter * 0.007f;
                npc.velocity.Y += npcYCenter * 0.007f;
                npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) + MathHelper.ToRadians(90);
                if (npc.velocity.X > 1.5)
                    npc.velocity.X *= 0.9f;

                if (npc.velocity.X < -1.5)
                    npc.velocity.X *= 0.9f;

                if (npc.velocity.Y > 1.5)
                    npc.velocity.Y *= 0.9f;

                if (npc.velocity.Y < -1.5)
                    npc.velocity.Y *= 0.9f;

                if (npc.velocity.X > 3f)
                    npc.velocity.X = 3f;

                if (npc.velocity.X < -3f)
                    npc.velocity.X = -3f;

                if (npc.velocity.Y > 3f)
                    npc.velocity.Y = 3f;

                if (npc.velocity.Y < -3f)
                    npc.velocity.Y = -3f;
            }
            else
            {
                if (npc.velocity.X < npcXCenter)
                {
                    npc.velocity.X += weight;
                    if (npc.velocity.X < 0f && npcXCenter > 0f)
                        npc.velocity.X += weight;
                }
                else if (npc.velocity.X > npcXCenter)
                {
                    npc.velocity.X -= weight;
                    if (npc.velocity.X > 0f && npcXCenter < 0f)
                        npc.velocity.X -= weight;
                }

                if (npc.velocity.Y < npcYCenter)
                {
                    npc.velocity.Y += weight;
                    if (npc.velocity.Y < 0f && npcYCenter > 0f)
                        npc.velocity.Y += weight;
                }
                else if (npc.velocity.Y > npcYCenter)
                {
                    npc.velocity.Y -= weight;
                    if (npc.velocity.Y > 0f && npcYCenter < 0f)
                        npc.velocity.Y -= weight;
                }

                npc.rotation = (float)Math.Atan2(npcYCenter, npcXCenter) + MathHelper.ToRadians(90);
            }

            float velocityMultiplier = 0.5f;
            if (npc.collideX)
            {
                npc.netUpdate = true;
                npc.velocity.X = npc.oldVelocity.X * (0f - velocityMultiplier);
                if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                    npc.velocity.X = 2f;

                if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                    npc.velocity.X = -2f;
            }

            if (npc.collideY)
            {
                npc.netUpdate = true;
                npc.velocity.Y = npc.oldVelocity.Y * (0f - velocityMultiplier);
                if (npc.velocity.Y > 0f && npc.velocity.Y < 1.5)
                    npc.velocity.Y = 2f;

                if (npc.velocity.Y < 0f && npc.velocity.Y > -1.5)
                    npc.velocity.Y = -2f;
            }

            if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
                npc.netUpdate = true;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int tileCoordX = (int)npc.Center.X / 16;
                int tileCoordY = (int)npc.Center.Y / 16;
                bool noEligibleWall = false;
                int displacement;
                for (int i = tileCoordX - 1; i <= tileCoordX + 1; i = displacement + 1)
                {
                    for (int j = tileCoordY - 1; j <= tileCoordY + 1; j = displacement + 1)
                    {
                        if (Main.tile[i, j] == null)
                        {
                            return;
                        }
                        if (Main.tile[i, j].wall > 0)
                        {
                            noEligibleWall = true;
                        }
                        displacement = j;
                    }
                    displacement = i;
                }
                if (!noEligibleWall)
                {
                    npc.Transform(ModContent.NPCType<SacSpiderFloored>());
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (++npc.frameCounter > 8)
            {
                if (++frame > 3)
                    frame = 0;
                npc.frame.Y = frame * frameHeight;
                npc.frameCounter = 0;
            }
        }
    }
}