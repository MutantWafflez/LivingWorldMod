namespace LivingWorldMod.Content.Subworlds.Pyramid {
    /// <summary>
    /// An enum of all Room Curses for the pyramid.
    /// </summary>
    public enum PyramidRoomCurseType {
        /// <summary>
        /// Player receives the Obstruction Debuff.
        /// </summary>
        Obstruction,

        /// <summary>
        /// 2x the amount of enemies in the room.
        /// </summary>
        Battle,

        /// <summary>
        /// Bottom half of the room is filled with water.
        /// </summary>
        Flooding,

        /// <summary>
        /// Taking damage will light the player on fire.
        /// </summary>
        Ignition,

        /// <summary>
        /// Any placed torches will be destroyed after 20 seconds.
        /// </summary>
        DyingLight,

        /// <summary>
        /// Player recieves the Confused Debuff.
        /// </summary>
        Confusion,

        /// <summary>
        /// Player must survive 45 seconds of non-stop enemy spawns.
        /// </summary>
        Siege,

        /// <summary>
        /// Player cannot gain health in any regard.
        /// </summary>
        Hemophilia,

        /// <summary>
        /// Player's defense is halved.
        /// </summary>
        ShatteredArmor,

        /// <summary>
        /// All enemies update 50% faster.
        /// </summary>
        Hyperactivity,

        /// <summary>
        /// Gravity randomly swaps for everyone individually in the room.
        /// </summary>
        GravitationalInstability,

        /// <summary>
        /// Taking damage will poison the player.
        /// </summary>
        Poison,

        /// <summary>
        /// Player will lose money when they take damage.
        /// </summary>
        Thievery,

        /// <summary>
        /// When the player is in the dark, they will take damage.
        /// </summary>
        Nyctophobia,

        /// <summary>
        /// Players can only see enemies within 14 tiles.
        /// </summary>
        Nearsightedness,

        /// <summary>
        /// Player uses items 50% slower.
        /// </summary>
        Lethargy,

        /// <summary>
        /// All enemies and players damage is multiplied by any value from 25% to 250%.
        /// </summary>
        Chaos,

        /// <summary>
        /// Player will hallucinate enemies and projectiles.
        /// </summary>
        Insanity,

        /// <summary>
        /// Player moves as if they are in water.
        /// </summary>
        Viscosity,

        /// <summary>
        /// All solid blocks act like honey blocks.
        /// </summary>
        Adhesion,

        /// <summary>
        /// Enemies will explode on death, damaging anyone within 8 tiles.
        /// </summary>
        Combustion,

        /// <summary>
        /// Enemies gain 5 DR stacks and are knockback resistant.
        /// </summary>
        IronCurtain,

        /// <summary>
        /// All enemy projectiles spawn with 2x velocity.
        /// </summary>
        DodgeOrDie,

        /// <summary>
        /// Players within 15 tiles of an enemy are slowed down.
        /// </summary>
        Proximity,

        /// <summary>
        /// Any player taking damage adds another curse up to a maximum of 5.
        /// </summary>
        Recursion,

        /// <summary>
        /// Game is forcefully zoomed to 400%.
        /// </summary>
        Spotlight,

        /// <summary>
        /// Player cannot see their health or mana.
        /// </summary>
        Insensitivity,

        /// <summary>
        /// Taking damage will disable an accessory for 30 seconds.
        /// </summary>
        Disarmament,

        /// <summary>
        /// Enemies will gain the defense of the nearest player.
        /// </summary>
        Reflection,

        /// <summary>
        /// All sounds are muted in the room.
        /// </summary>
        Silence,

        /// <summary>
        /// Every 15 seconds, all currently active curses (excluding this one) change.
        /// </summary>
        Rotation,

        /// <summary>
        /// Knockback prevention is removed for Players; knockback is 100% more potent
        /// </summary>
        Impact,

        /// <summary>
        /// Colliding with an object fast enough will injure the player.
        /// </summary>
        Kinetics,

        /// <summary>
        /// Player will randomly "trip", take damage and slow down. Players will jump with a random force of 10% to 150%.
        /// Enemies will randomly double update.
        /// </summary>
        MinorInconveniences,

        /// <summary>
        /// Healing via a consumable item will take 2 seconds after consumption to actually heal the player.
        /// </summary>
        Delay,

        /// <summary>
        /// All tiles have no friction while being walked across.
        /// </summary>
        FrictionalIgnorance,

        /// <summary>
        /// When the player's velocity becomes positive (falling), gravity acceleration is 300% more potent.
        /// </summary>
        Grounding,

        /// <summary>
        /// All boons are locked into their slots and no longer function.
        /// </summary>
        DivineSilence,

        /// <summary>
        /// Every 20 seconds, players will be given a random debuff for 10 seconds. Only function if player is >33% life.
        /// </summary>
        ThePlagues,

        /// <summary>
        /// 150% more traps in the room, and players take 100% damage from traps.
        /// </summary>
        UnsteadyFooting,

        /// <summary>
        /// When an enemy is killed, the nearest player gains the Pacifist's Plight debuff for 5 seconds.
        /// (34% less damage, defense, and damage reduction)
        /// </summary>
        Pacifism,

        /// <summary>
        /// Any projectile shot by a player has a chance to fire inaccurately.
        /// </summary>
        Misfire
    }
}