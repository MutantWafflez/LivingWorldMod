using System.IO;
using System.Linq;
using LivingWorldMod.Globals.BaseTypes.Systems;
using LivingWorldMod.Utilities;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Events;
using Terraria.ModLoader.IO;
using Party = Terraria.GameContent.Events.BirthdayParty;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     ModSystem that handles special "night" parties that happen in tandem with
///     lantern nights.
/// </summary>
public sealed class NightPartySystem : BaseModSystem<NightPartySystem> {
    private bool _shouldNightParty;

    private bool _nextLanternNightShouldAlsoBeParty;

    private static void StartNightParty() {
        Party.GenuineParty = true;
        Party.PartyDaysOnCooldown = Main.rand.Next(1, 3);
        Party.CelebratingNPCs = LWMUtils.GetAllNPCs(Party.CanNPCParty).Select(npc => npc.whoAmI).ToList();
        NPC.freeCake = true;

        WorldGen.BroadcastText("Event.NightPartyStarted".Localized().ToNetworkText(), LWMUtils.DarkPinkPartyTextColor);

        NetMessage.SendData(MessageID.WorldData);
        AchievementsHelper.NotifyProgressionEvent(25);
    }

    public override void PreUpdateTime() {
        _shouldNightParty = LanternNight.NextNightIsLanternNight;
    }

    public override void PostUpdateTime() {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        if (_shouldNightParty && !Main.dayTime && Main.time == 0) {
            if (_nextLanternNightShouldAlsoBeParty) {
                StartNightParty();
                
                return;
            }

            _nextLanternNightShouldAlsoBeParty = !_nextLanternNightShouldAlsoBeParty;
        }
        else if (Main.dayTime && Main.time == 0) {
            // Method doesn't do any time checks itself, it will just stop a party if one is happening
            Party.CheckNight();
        }
    }

    public override void SaveWorldData(TagCompound tag) {
        tag[nameof(_nextLanternNightShouldAlsoBeParty)] = _nextLanternNightShouldAlsoBeParty;
    }

    public override void LoadWorldData(TagCompound tag) {
        _nextLanternNightShouldAlsoBeParty = tag.GetBool(nameof(_nextLanternNightShouldAlsoBeParty));
    }
}