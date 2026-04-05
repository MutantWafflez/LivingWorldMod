using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patch class that handles the necessary patches/detours for stitching together Vanilla's <see cref="TownRoomManager" /> and LWM's <see cref="TownNPCTownSystem" />
/// </summary>
public class TownSystemPatches : LoadablePatch {
    public override void LoadPatches() {
        On_TownRoomManager.AddOccupantsToList_Point_List1 += OnAddOccuputantsRefreshLWMTowns;
        On_TownRoomManager.SetRoom_int_Point += OnSetRoomRefreshLWMTowns;
        On_TownRoomManager.KickOut_int += OnKickOutRefreshLWMTowns;
    }

    private void OnAddOccuputantsRefreshLWMTowns(On_TownRoomManager.orig_AddOccupantsToList_Point_List1 orig, TownRoomManager self, Point tilePosition, List<int> occupants) {
        orig(self, tilePosition, occupants);

        TownNPCTownSystem.Instance.CalculateTowns();
    }

    private void OnSetRoomRefreshLWMTowns(On_TownRoomManager.orig_SetRoom_int_Point orig, TownRoomManager self, int npcID, Point pt) {
        orig(self, npcID, pt);

        TownNPCTownSystem.Instance.CalculateTowns();
    }

    private void OnKickOutRefreshLWMTowns(On_TownRoomManager.orig_KickOut_int orig, TownRoomManager self, int npcType) {
        orig(self, npcType);

        TownNPCTownSystem.Instance.CalculateTowns();
    }
}