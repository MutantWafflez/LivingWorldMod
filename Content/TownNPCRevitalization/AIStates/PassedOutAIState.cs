using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

/// <summary>
///     Town NPC AI state that occurs when an NPC has reached the maximum amount of ticks awake, and "passes out" due to exhaustion.
/// </summary>
public class PassedOutAIState : TownNPCAIState {
    public override void DoState(TownGlobalNPC globalNPC, NPC npc) {
        TownNPCSleepModule sleepModule = globalNPC.SleepModule;
        npc.direction = 1;
        npc.rotation = MathHelper.PiOver2;

        TownNPCSpriteModule spriteModule = globalNPC.SpriteModule;
        spriteModule.CloseEyes();
        spriteModule.RequestDraw(TownNPCSleepModule.GetSleepSpriteDrawData with { color = Color.Red * 0.8f });
        spriteModule.OffsetDrawPosition(new Vector2(0, npc.width));

        TownNPCChatModule chatModule = globalNPC.ChatModule;
        chatModule.DisableChatting(LWMUtils.RealLifeSecond);
        chatModule.DisableChatReception(LWMUtils.RealLifeSecond);

        sleepModule.isAsleep = true;
        if ((sleepModule.awakeTicks -= 2f) <= 0) {
            TownGlobalNPC.RefreshToState<DefaultAIState>(npc);
        }
    }
}