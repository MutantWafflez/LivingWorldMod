using System.Collections.Generic;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace LivingWorldMod.Globals.BaseTypes.Systems;

/// <summary>
///     Unique type of ModSystem that can be extended for automatic setting
///     up and handling of the given UIState T.
/// </summary>
[Autoload(Side = ModSide.Client)]
public abstract class UISystem<TSystem, TState> : BaseModSystem<TSystem> where TState : UIState, new() where TSystem : BaseModSystem<TSystem> {
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
    public virtual string InternalInterfaceName => LWMUtils.SplitBetweenCapitalLetters(typeof(TState).Name).Trim();

    /// <summary>
    ///     What kind of scale type this interface will be using. Defaults to InterfaceScaleType.UI.
    /// </summary>
    public virtual InterfaceScaleType ScaleType => InterfaceScaleType.UI;

    public override void SetStaticDefaults() {
        _userInterface = new UserInterface();

        Player.Hooks.OnEnterWorld += InitializeUIState;
    }

    public override void Unload() {
        Player.Hooks.OnEnterWorld -= InitializeUIState;
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
    ///     Initializes the <see cref="UIState" /> by calling its constructor. This is done when the player first enters any world.
    /// </summary>
    /// <remarks>
    ///     If you wish to override this method, make sure to still call this base method, or at the very least initialize <see cref="UIState" />. If you don't, the UI will not work.
    ///     <para></para>
    ///     As for why this is necessary, the TL;DR is that in the main menu, <see cref="Main.UIScale" /> is set to a different value than what it is in-game/world. As such, any initialization
    ///     of the UI before the world is entered that relies on dimension calculations will return incorrect values, and thus break.
    /// </remarks>
    protected virtual void InitializeUIState(Player player) {
        if (player.whoAmI != Main.myPlayer || UIState is not null) {
            return;
        }

        UIState = new TState();
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