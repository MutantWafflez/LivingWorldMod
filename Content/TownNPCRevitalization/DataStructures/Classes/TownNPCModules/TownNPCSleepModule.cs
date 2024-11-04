using System;
using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

public sealed  class TownNPCSleepModule  : TownNPCModule {
    private const int MaxAwakeValue = LWMUtils.InGameHour * 24;
    private const float DefaultAwakeValue = MaxAwakeValue * 0.2f;

    private static readonly SleepSchedule DefaultSleepSchedule = new(new TimeOnly(19, 30, 0), new TimeOnly(4, 30, 0));

    private static readonly bool GenuinePartyIsOccurring = BirthdayParty.PartyIsUp && BirthdayParty.GenuineParty;

    /// <summary>
    ///     The amount of ticks that this NPC has been awake. The higher the value, the more severe effects on mood will occur. This value is decreased rapidly by a Town NPC sleeping.
    /// </summary>
    public BoundedNumber<float> awakeTicks = new(DefaultAwakeValue, 0, MaxAwakeValue);

    public bool isAsleep;

    public static DrawData GetSleepSpriteDrawData {
        get {
            Main.instance.LoadItem(ItemID.SleepingIcon);
            Texture2D sleepingIconTexture = TextureAssets.Item[ItemID.SleepingIcon].Value;
            return new DrawData(sleepingIconTexture, new Vector2(sleepingIconTexture.Width / -2f, -32f + MathF.Sin(Main.GlobalTimeWrappedHourly)), Color.White * 0.67f);
        }
    }

    public bool ShouldSleep {
        get {
            bool sleepBeingBlocked = LanternNight.LanternsUp
                // TODO: Allow sleeping once tired enough, even if party is occurring
                || GenuinePartyIsOccurring
                || Main.slimeRain
                || Main.invasionType > InvasionID.None
                || Main.bloodMoon
                || Main.snowMoon
                || Main.pumpkinMoon
                || globalNPC.ChatModule.IsChattingWithPlayerDirectly;
            SleepSchedule npcSleepSchedule = GetSleepProfileOrDefault(npc.type);

            return !sleepBeingBlocked && LWMUtils.CurrentInGameTime.IsBetween(npcSleepSchedule.StartTime, npcSleepSchedule.EndTime);
        }
    }

    public TownNPCSleepModule(NPC npc, TownGlobalNPC globalNPC) : base(npc, globalNPC) {
        globalNPC.OnSave += tag => tag[nameof(awakeTicks)] = awakeTicks.Value;
        globalNPC.OnLoad += tag => awakeTicks = new BoundedNumber<float>(
            tag.TryGet(nameof(awakeTicks), out float savedSleepValue) ? savedSleepValue : DefaultAwakeValue,
            0,
            MaxAwakeValue
        );
    }

    public static SleepSchedule GetSleepProfileOrDefault(int npcType) => TownNPCDataSystem.sleepSchedules.GetValueOrDefault(npcType, DefaultSleepSchedule);

    public override void Update() {
        if (!isAsleep) {
            awakeTicks += 1f;
        }

        isAsleep = false;
        if (awakeTicks < MaxAwakeValue) {
            return;
        }

        globalNPC.PathfinderModule.CancelPathfind();
        TownGlobalNPC.RefreshToState<PassedOutAIState>(npc);
    }
}