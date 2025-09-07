using System;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs.Animations;

/// <summary>
///     Represents the "walk" animation of
/// </summary>
/// <param name="startFrame"></param>
/// <param name="endFrame"></param>
/// <param name="frameDuration"></param>
public struct WalkAnimation(int startFrame, int endFrame, int frameDuration, int priority = 0) : IAnimation {
    public const int TownDogStartFrame = 9;
    public const int TownBunnyStartFrame = 1;
    public const int DefaultStartFrame = 2;

    public const int TownDogAndBunnyFrameDuration = 12;
    public const int DefaultFrameDuration = 6;

    public bool IsFinished {
        get;
        private set;
    }

    public int Priority {
        get;
    } = priority;

    public WalkAnimation(NPC npc) : this(
        npc.type switch {
            NPCID.TownDog => TownDogStartFrame,
            NPCID.TownBunny => TownBunnyStartFrame,
            _ => DefaultStartFrame
        },
        Main.npcFrameCount[npc.type] - NPCID.Sets.ExtraFramesCount[npc.type],
        npc.type switch {
            NPCID.TownDog or NPCID.TownBunny => TownDogAndBunnyFrameDuration,
            _ => DefaultFrameDuration
        }
    ) { }

    public void Update(NPC npc, int frameHeight) {
        if (npc.velocity.X == 0f) {
            IsFinished = true;

            return;
        }

        npc.frameCounter += Math.Abs(npc.velocity.X) * 2f + 1f;

        int startFrameY = startFrame * frameHeight;
        if (npc.frame.Y < startFrameY) {
            npc.frame.Y = startFrameY;
        }

        if (npc.frameCounter > frameDuration) {
            npc.frame.Y += frameHeight;
            npc.frameCounter = 0.0;
        }

        if (npc.frame.Y >= endFrame * frameHeight) {
            npc.frame.Y = startFrameY;
        }
    }
}