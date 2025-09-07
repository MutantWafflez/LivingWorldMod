using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records.Animations;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

/// <summary>
///     Town NPC Module that deals specifically with animations/framing for Town NPCs. This module handles no additional drawing; that is for <see cref="TownNPCSpriteModule" /> to handle.
/// </summary>
public class TownNPCAnimationModule : TownNPCModule {
    private IAnimation _currentAnimation;
    public override int UpdatePriority => -2;

    public override void UpdateModule() {
        if (NPC.velocity.X == 0f) {
            return;
        }

        RequestAnimation(new WalkAnimation(NPC));
    }

    public override void FindFrame(NPC npc, int frameHeight) {
        NPC.spriteDirection = npc.direction;

        if (!(_currentAnimation?.Update(npc, frameHeight) ?? false)) {
            return;
        }

        _currentAnimation = null;
        npc.frame.Y = (int)(npc.frameCounter = 0);
    }

    public void RequestAnimation(IAnimation newAnimation) {
        if (_currentAnimation is null || _currentAnimation.Priority >= newAnimation.Priority) {
            _currentAnimation = newAnimation;
        }
    }
}