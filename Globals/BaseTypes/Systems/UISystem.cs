using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace LivingWorldMod.Globals.BaseTypes.Systems;

/// <summary>
///     Unique type of ModSystem that can be extended for automatic setting
///     up and handling of the given UIState T.
/// </summary>
[Autoload(Side = ModSide.Client)]
public abstract partial class UISystem<TSystem, TState> : BaseModSystem<TSystem> where TState : UIState, new() where TSystem : BaseModSystem<TSystem> {
    public UserInterface correspondingInterface;

    public TState correspondingUIState;

    protected GameTime lastGameTime;

    /// <summary>
    ///     The name of the Vanilla Interface to place this UI BEFORE.
    ///     Defaults to Mouse Text.
    /// </summary>
    public virtual string VanillaInterfaceLocation => "Vanilla: Mouse Text";

    /// <summary>
    ///     The internal name of this Interface when inserting into the Interface
    ///     Layers. Defaults to the name of the passed in UIState.
    /// </summary>
    public virtual string InternalInterfaceName => SpaceBetweenCapitalsRegex().Replace(typeof(TState).Name, " $1").Trim();

    /// <summary>
    ///     What kind of scale type this interface will be using. Defaults to InterfaceScaleType.UI.
    /// </summary>
    public virtual InterfaceScaleType ScaleType => InterfaceScaleType.UI;

    public override void SetStaticDefaults() {
        correspondingInterface = new UserInterface();
        correspondingUIState = new TState();

        correspondingUIState.Activate();
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        int specifiedIndex = layers.FindIndex(layer => layer.Name.Equals(VanillaInterfaceLocation));
        if (specifiedIndex != -1) {
            layers.Insert(
                specifiedIndex,
                new LegacyGameInterfaceLayer(
                    "LWM: " + InternalInterfaceName,
                    delegate {
                        if (lastGameTime is not null && correspondingInterface.CurrentState is not null) {
                            correspondingInterface.Draw(Main.spriteBatch, lastGameTime);
                        }

                        return true;
                    },
                    ScaleType
                )
            );
        }
    }

    public override void UpdateUI(GameTime gameTime) {
        lastGameTime = gameTime;
        if (lastGameTime is not null && correspondingInterface.CurrentState == correspondingUIState) {
            correspondingInterface.Update(lastGameTime);
        }
    }

    [GeneratedRegex("([A-Z])")]
    private static partial Regex SpaceBetweenCapitalsRegex();
}