using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

/// <summary>
///     Town NPC AI state that occurs when an NPC has reached the maximum amount of ticks awake, and "passes out" due to exhaustion.
/// </summary>
public class PassedOutAIState : TownNPCAIState {
    public override void DoState( NPC npc) {
        TownNPCSleepModule sleepModule = npc.GetGlobalNPC<TownNPCSleepModule>();
        npc.direction = 1;
        npc.rotation = MathHelper.PiOver2;

        TownNPCSpriteModule spriteModule = npc.GetGlobalNPC<TownNPCSpriteModule>();
        spriteModule.CloseEyes();
        spriteModule.RequestDraw(sleepModule.GetSleepSpriteDrawData with { Color = Color.Red * 0.8f });
        spriteModule.OffsetDrawPosition(new Vector2(0, npc.width));

        TownNPCChatModule chatModule = npc.GetGlobalNPC<TownNPCChatModule>();
        chatModule.DisableChatting(LWMUtils.RealLifeSecond);
        chatModule.DisableChatReception(LWMUtils.RealLifeSecond);

        if ((sleepModule.awakeTicks -= 2f * (float)Main.dayRate) <= 0) {
            TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
        }
    }
}