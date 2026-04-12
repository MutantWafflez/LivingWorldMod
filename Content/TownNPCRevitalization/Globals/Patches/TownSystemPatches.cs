using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patch class that handles the necessary patches/detours for stitching together Vanilla's <see cref="TownRoomManager" /> and LWM's <see cref="TownNPCTownSystem" />
/// </summary>
public class TownSystemPatches : LoadablePatch {
    private static Point GetRoomBeforeRemoval(TownRoomManager roomManager, int npcType) {
        Point roomPos;
        lock (TownRoomManager.EntityCreationLock) {
            roomPos = roomManager._roomLocationPairs.FirstOrDefault(pair => pair.Item1 == npcType)?.Item2 ?? Point.Zero;
        }

        return roomPos;
    }

    private static bool AnyValidNPCTypes(List<int> types) => types.Any(TownNPCTownSystem.NPCTypeIsValidForTownInclusion);

    private static List<int> GetTypesInRoom(TownRoomManager roomManager, Point roomPos) {
        return roomManager._roomLocationPairs.Where(pair => pair.Item2 == roomPos).Select(pair => pair.Item1).ToList();
    }

    public override void LoadPatches() {
        On_TownRoomManager.SetRoom_int_Point += OnSetRoomRefreshLWMTowns;
        On_TownRoomManager.KickOut_int += OnKickOutRefreshLWMTowns;
    }

    private void OnSetRoomRefreshLWMTowns(On_TownRoomManager.orig_SetRoom_int_Point orig, TownRoomManager self, int npcID, Point pt) {
        Point previousRoomPos = GetRoomBeforeRemoval(self, npcID);

        orig(self, npcID, pt);

        List<int> typesLeftInPrevRoom = GetTypesInRoom(self, previousRoomPos);
        if (!AnyValidNPCTypes(typesLeftInPrevRoom)) {
            TownNPCTownSystem.Instance.RemoveRoomFromTown(previousRoomPos);
        }

        List<int> typesInNewRoom = GetTypesInRoom(self, pt);
        if (!AnyValidNPCTypes(typesInNewRoom)) {
            TownNPCTownSystem.Instance.AddRoomToTown(pt);
        }
    }

    private void OnKickOutRefreshLWMTowns(On_TownRoomManager.orig_KickOut_int orig, TownRoomManager self, int npcType) {
        Point roomPos = GetRoomBeforeRemoval(self, npcType);

        orig(self, npcType);

        List<int> typesLeftInRoom = GetTypesInRoom(self, roomPos);
        // Only remove the room if there are either no NPCs left in that room, or the remaining NPCs do not adhere to the Revitalization (Town Slimes and some pets at this moment)
        // TODO: Update this comment when this changes ^
        if (roomPos == Point.Zero || AnyValidNPCTypes(typesLeftInRoom)) {
            return;
        }

        TownNPCTownSystem.Instance.RemoveRoomFromTown(roomPos);
    }
}