using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.DebugModules;

public class TownNPCDebugModule : DebugModule {
    private NPC _selectedNPC;

    public override void ModuleUpdate() {
        if (Main.mouseLeft && Main.mouseLeftRelease)  {
            foreach (NPC npc in Main.ActiveNPCs) {
                if (!npc.Hitbox.Contains(Main.MouseWorld.ToPoint())) {
                    continue;
                }

                if (_selectedNPC == npc)  {
                    _selectedNPC = null;
                    Main.NewText($"Deselected NPC: {npc}");
                    break;
                }

                _selectedNPC = npc;
                Main.NewText($"Selected NPC: {npc}");
                break;
            }
        }

        if (_selectedNPC is null || !_selectedNPC.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
            return;
        }

        Dust.QuickBox(_selectedNPC.TopLeft, _selectedNPC.BottomRight, 2, Main.DiscoColor, null);
        if (Main.mouseRight && Main.mouseRightRelease) {
            Point pathfindLocation = Main.MouseWorld.ToTileCoordinates();
            TownNPCStateModule.RefreshToState<WalkToRandomPosState>(_selectedNPC);
            _selectedNPC.ai[2] = 1f;

            _selectedNPC.GetGlobalNPC<TownNPCPathfinderModule>().CancelPathfind(_selectedNPC);
            _selectedNPC.GetGlobalNPC<TownNPCPathfinderModule>().RequestPathfind(_selectedNPC, pathfindLocation);

            Main.NewText($"Pathfinding to {pathfindLocation}");
        }
    }

    public override void KeysPressed(Keys[] pressedKeys) {
        if (_selectedNPC is null || !_selectedNPC.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
            return;
        }

        if (pressedKeys.Contains(Keys.NumPad1)) {
            Main.NewText("Forcing NPC to Pass out");

            _selectedNPC.GetGlobalNPC<TownNPCPathfinderModule>().CancelPathfind(_selectedNPC);
            TownNPCStateModule.RefreshToState<PassedOutAIState>(_selectedNPC);
        }
    }
}