using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.DataStructures.Classes;
using MonoMod.Cil;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

public class NPCChatGUIPatches : LoadablePatch {
    public override void LoadPatches() {
        IL_Main.GUIChatDrawInner += HappinessUIPatch;
    }

    private void HappinessUIPatch(ILContext il) {
        // Patch that opens up our Happiness UI instead of displaying the NPC text.
        currentContext = il;

        ILCursor c = new(il);

        c.GotoNext(i => i.MatchLdsfld<Main>(nameof(Main.npcChatFocus4)));
        c.GotoNext(i => i.MatchRet());
        c.EmitDelegate(() => {
            Main.npcChatText = "";
            ModContent.GetInstance<HappinessUISystem>().OpenHappinessState(Main.LocalPlayer.TalkNPC);
        });
    }
}