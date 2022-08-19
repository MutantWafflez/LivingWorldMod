using LivingWorldMod.Common.Players;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Accessories {
    /// <summary>
    /// ModItem class for specifically accessories that works in direct tandem with <seealso cref="AccessoryPlayer"/>.
    /// </summary>
    public abstract class AccessoryItem : BaseItem {
        /// <summary>
        /// The priority of this effect, or where in the order of accessories
        /// this effect will be triggered. Defaults to 0.
        /// </summary>
        public int EffectPriority {
            get;
            protected set;
        } = 0;

        /// <summary>
        /// Equivalent to <seealso cref="ModItem.UpdateAccessory"/>.
        /// </summary>
        /// <param name="player"> The player that has equipped this accessory. </param>
        /// <param name="hideVisual"> Whether or not this player has marked this accessory to be visible on them. </param>
        public virtual void AccessoryUpdate(Player player, bool hideVisual) { }

        /// <summary>
        /// Equivalent to <seealso cref="ModPlayer.ResetEffects"/>. This should only be used
        /// if your accessory has additional data that needs to be reset each update cycle.
        /// </summary>
        public virtual void ResetAccessoryEffects(Player player) { }

        /// <summary>
        /// This hook is called whenever the player is about to be killed after reaching 0 health. Set the playSound parameter to false to stop the death sound from playing.
        /// Set the genGore parameter to false to stop the gore and dust from being created. (These are useful for creating your own sound or gore.)
        /// Return false to stop the player from being killed. Only return false if you know what you are doing! Returns true by default.
        /// </summary>
        public virtual bool PrePlayerKill(Player player, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) => true;

        /// <summary>
        /// This method is called when this mod attempts to forcefully kill the player for any reason. Returning false prevents the death.
        /// Returns true by default.
        /// </summary>
        public virtual bool PrePlayerForceKill(Player player, PlayerDeathReason deathReason) => true;

        /// <summary>
        /// Allows you to make anything happen when the player dies.
        /// </summary>
        public virtual void PlayerKill(Player player, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) { }

        /// <summary>
        /// This method is called during <seealso cref="ModPlayer.PreKill"/> at ONLY the first instance in which an
        /// accessory negates death.
        /// </summary>
        public virtual void PlayerDeathNegated(Player player) { }

        /// <summary>
        /// This hook is called before every time the player takes damage. The pvp parameter is whether the damage was from another player.
        /// The quiet parameter determines whether the damage will be communicated to the server. The damage, hitDirection, and crit parameters can be modified.
        /// Set the customDamage parameter to true if you want to use your own damage formula (this parameter will disable automatically subtracting the player's defense from the damage).
        /// Set the playSound parameter to false to disable the player's hurt sound, and the genGore parameter to false to disable the dust particles that spawn.
        /// (These are useful for creating your own sound or gore.) The deathText parameter can be modified to change the player's death message if the player dies.
        /// Return false to stop the player from taking damage. Returns true by default.
        /// </summary>
        public virtual bool PrePlayerHurt(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) => true;

        /// <summary>
        /// Allows you to make anything happen right before damage is subtracted from the player's health.
        /// </summary>
        public virtual void PlayerHurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) { }

        /// <summary>
        /// Allows you to make anything happen when the player takes damage.
        /// </summary>
        public virtual void PostPlayerHurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) { }

        public override void SetDefaults() {
            Item.DefaultToAccessory();
        }

        public sealed override void UpdateAccessory(Player player, bool hideVisual) {
            GetAccPlayer(player).equippedModAccessories.Add(this);

            AccessoryUpdate(player, hideVisual);
        }

        /// <summary>
        /// Gets and returns the <seealso cref="AccessoryPlayer"/> instance that belongs to this player.
        /// </summary>
        public AccessoryPlayer GetAccPlayer(Player player) => player.GetModPlayer<AccessoryPlayer>();
    }
}