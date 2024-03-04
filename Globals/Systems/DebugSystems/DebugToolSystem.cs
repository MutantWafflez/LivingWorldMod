using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Custom.Classes.DebugModules;
using Microsoft.Xna.Framework.Input;

namespace LivingWorldMod.Globals.Systems.DebugSystems;

/// <summary>
/// ModSystem that only loads in debug mode that uses <seealso cref="DebugModule"/> objects for functionality.
/// </summary>
[Autoload(Side = ModSide.Client)]
public class DebugToolSystem : ModSystem {
    public DebugModule CurrentModule => _allModules[_currentModuleIndex];

    private int _currentModuleIndex;

    private List<DebugModule> _allModules;

    public override bool IsLoadingEnabled(Mod mod) => LWM.IsDebug;

    public override void Load() {
        _allModules = new List<DebugModule> {
            new StructureModule(),
            new SkipWallModule(),
            new SkipTileModule()
        };
    }

    public override void PostUpdateEverything() {
        //Trigger module swap on Numpad 0 press
        if (Main.keyState.IsKeyDown(Keys.NumPad0) && !Main.oldKeyState.IsKeyDown(Keys.NumPad0)) {
            _currentModuleIndex++;

            if (_currentModuleIndex >= _allModules.Count) {
                _currentModuleIndex = 0;
            }
            Main.NewText($"Current Module: {CurrentModule.GetType().Name}");
        }

        //Delegate key presses to current module
        CurrentModule.KeysPressed(Main.keyState.GetPressedKeys().Where(key => !Main.oldKeyState.GetPressedKeys().Contains(key)).ToArray());

        //Update module
        CurrentModule.ModuleUpdate();
    }
}