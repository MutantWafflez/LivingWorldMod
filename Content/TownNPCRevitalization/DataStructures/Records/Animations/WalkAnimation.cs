using System;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records.Animations;

/// <summary>
///     Represents the "walk" animation of Town NPC, starting at <see cref="StartFrame" />, incrementing after <see cref="FrameDuration" /> is reached, until <see cref="EndFrame" /> is met,
///     where it then repeats until the NPC stops moving horizontally.
/// </summary>
public readonly record struct WalkAnimation(int StartFrame, int EndFrame, int FrameDuration, int Priority = 0) : IAnimation {
    public const int TownDogStartFrame = 9;
    public const int TownBunnyStartFrame = 1;
    public const int DefaultStartFrame = 2;

    public const int TownDogAndBunnyFrameDuration = 12;
    public const int DefaultFrameDuration = 6;

    public int Priority {
        get;
    } = Priority;

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

    public void Start(NPC npc, int frameHeight) {
        npc.frame.Y = StartFrame * frameHeight;
        npc.frameCounter = 0;
    }

    public bool Update(NPC npc, int frameHeight) {
        if (npc.velocity.X == 0f) {
            return true;
        }

        npc.frameCounter += Math.Abs(npc.velocity.X) * 2f + 1f;

        if (npc.frameCounter > FrameDuration) {
            npc.frame.Y += frameHeight;
            npc.frameCounter = 0.0;
        }

        if (npc.frame.Y >= EndFrame * frameHeight) {
            npc.frame.Y = StartFrame * frameHeight;
        }

        return false;
    }
}