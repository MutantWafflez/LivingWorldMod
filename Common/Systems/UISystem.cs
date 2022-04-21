using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// Unique type of ModSystem that can be extended for automatic setting
    /// up and handling of the given UIState T.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public abstract class UISystem<T> : ModSystem where T : UIState, new() {
        public UserInterface correspondingInterface;

        public T correspondingUIState;

        /// <summary>
        /// The name of the Vanilla Interface to place this UI BEFORE.
        /// Defaults to Mouse Text.
        /// </summary>
        public virtual string VanillaInterfaceLocation => "Vanilla: Mouse Text";

        /// <summary>
        /// The internal name of this Interface when inserting into the Interface
        /// Layers. Defaults to the name of the passed in UIState.
        /// </summary>
        public virtual string InternalInterfaceName => typeof(T).Name;

        protected GameTime lastGameTime;

        public override void Load() {
            correspondingInterface = new UserInterface();
            correspondingUIState = new T();

            correspondingUIState.Activate();
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int specifiedIndex = layers.FindIndex(layer => layer.Name.Equals(VanillaInterfaceLocation));
            if (specifiedIndex != -1) {
                layers.Insert(specifiedIndex, new LegacyGameInterfaceLayer(
                    "LWM: " + InternalInterfaceName,
                    delegate {
                        if (lastGameTime is not null && correspondingInterface.CurrentState is not null) {
                            correspondingInterface.Draw(Main.spriteBatch, lastGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        public override void UpdateUI(GameTime gameTime) {
            lastGameTime = gameTime;
            if (lastGameTime is not null && correspondingInterface.CurrentState == correspondingUIState) {
                correspondingUIState.Update(lastGameTime);
            }
        }
    }
}