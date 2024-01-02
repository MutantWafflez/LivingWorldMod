using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Content.TownNPCAIStates;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;

namespace LivingWorldMod.Content.TownNPCActivities;

public class ReleaseLanternActivity : TownNPCActivity {
    public override int ReservedStateInteger => 28;

    public override void DoState(TownAIGlobalNPC globalNPC, NPC npc) {
        switch (npc.ai[1]) {
            case 0f when ++npc.ai[2] > 45: {
                npc.ai[1] = 1f;

                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    Projectile.NewProjectileDirect(
                        npc.GetSource_Misc("ReleaseLantern"),
                        npc.position + new Vector2(16f, 0f) * npc.direction,
                        new Vector2(npc.direction, -0.5f) * Main.rand.NextFloat(1f, 3f),
                        ProjectileID.ReleaseLantern,
                        0,
                        0
                    );
                }
                break;
            }
            case 1f when ++npc.ai[2] > 90:
                npc.ai[0] = npc.ai[1] = npc.ai[2] = GetStateInteger<DefaultAIState>();
                break;
        }
    }

    public override bool CanDoActivity(TownAIGlobalNPC globalNPC, NPC npc) => LanternNight.LanternsUp;

    public override void FrameNPC(TownAIGlobalNPC globalNPC, NPC npc, int frameHeight) {
        int nonAttackFrameCount = Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type];
        switch (npc.ai[2]) {
            case < 15:
            case > 75:
                npc.frame.Y = frameHeight * (nonAttackFrameCount - 5);
                break;
            case < 45:
                npc.frame.Y = frameHeight * (nonAttackFrameCount - 4);
                break;
        }
    }
}