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
    public TState UIState;

    private GameTime _lastGameTime;
    private UserInterface _userInterface;

    /// <summary>
    ///     Whether or not this current UI is "active", i.e. <see cref="UserInterface.CurrentState" /> is not null.
    /// </summary>
    public bool UIIsActive => _userInterface.CurrentState is not null;

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

    [GeneratedRegex("([A-Z])")]
    private static partial Regex SpaceBetweenCapitalsRegex();

    public override void SetStaticDefaults() {
        _userInterface = new UserInterface();
        UIState = new TState();
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        int specifiedIndex = layers.FindIndex(layer => layer.Name.Equals(VanillaInterfaceLocation));
        if (specifiedIndex != -1) {
            layers.Insert(
                specifiedIndex,
                new LegacyGameInterfaceLayer(
                    "LWM: " + InternalInterfaceName,
                    InterfaceDraw,
                    ScaleType
                )
            );
        }
    }

    public override void UpdateUI(GameTime gameTime) {
        _lastGameTime = gameTime;
        if (_lastGameTime is not null && _userInterface.CurrentState == UIState) {
            _userInterface.Update(_lastGameTime);
        }
    }

    /// <summary>
    ///     Automatically activates <see cref="UIState" /> and sets the <see cref="_userInterface" />'s ActiveState to <see cref="UIState" />.
    /// </summary>
    protected void OpenUIState() {
        UIState.Activate();
        _userInterface.SetState(UIState);
    }

    /// <summary>
    ///     Automatically deactivates <see cref="UIState" /> and sets <see cref="_userInterface" />'s ActiveState to null.
    /// </summary>
    protected void CloseUIState() {
        UIState.Deactivate();
        _userInterface.SetState(null);
    }

    private bool InterfaceDraw() {
        if (_lastGameTime is not null && UIIsActive) {
            _userInterface.Draw(Main.spriteBatch, _lastGameTime);
        }

        return true;
    }
}