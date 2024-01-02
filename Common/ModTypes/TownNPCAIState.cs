using LivingWorldMod.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.ModTypes {
    /// <summary>
    /// ModType that denotes a certain state of being for Town NPCs.
    /// </summary>
    public abstract class TownNPCAIState : ModType {
        /// <summary>
        /// The internal numeric type of this state. Auto-assigned at initialization.
        /// </summary>
        public byte Type {
            get;
            private set;
        }

        /// <summary>
        /// The count of all TownNPCAIState extensions/objects that exist.
        /// </summary>
        public static byte StateCount {
            get;
            private set;
        }

        /// <summary>
        /// This is the number that will be used to denote that the Town NPC
        /// is in this AI state. Different AI State classes cannot have a same value
        /// for this property.
        /// </summary>
        public abstract int ReservedStateInteger {
            get;
        }

        /// <summary>
        /// Called every draw frame that the given Town NPC is doing the given activity
        /// (see <see cref="DoActivity"/>). Use this to frame the NPC to a specific animation
        /// frame for this activity.
        /// </summary>
        /// <remarks>
        /// The <see cref="TownAIGlobalNPC"/> parameter is added for your convenience, so you don't have to manually call
        /// <see cref="NPC.GetGlobalNPC"/>.
        /// </remarks>
        public virtual void FrameNPC(TownAIGlobalNPC globalNPC, NPC npc, int frameHeight) { }

        /// <summary>
        /// Called every draw frame that the given Town NPC is doing the given activity
        /// (see <see cref="DoActivity"/>). Use this to draw things BEHIND the NPC.
        /// </summary>
        /// <remarks>
        /// The <see cref="TownAIGlobalNPC"/> parameter is added for your convenience, so you don't have to manually call
        /// <see cref="NPC.GetGlobalNPC"/>.
        /// </remarks>
        public virtual void PreDrawNPC(TownAIGlobalNPC globalNPC, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

        /// <summary>
        /// Called every draw frame that the given Town NPC is doing the given activity
        /// (see <see cref="DoActivity"/>). Use this to draw things IN FRONT OF the NPC.
        /// </summary>
        /// <remarks>
        /// The <see cref="TownAIGlobalNPC"/> parameter is added for your convenience, so you don't have to manually call
        /// <see cref="NPC.GetGlobalNPC"/>.
        /// </remarks>
        public virtual void PostDrawNPC(TownAIGlobalNPC globalNPC, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

        public sealed override void SetupContent() => SetStaticDefaults();

        /// <summary>
        /// Called every tick that the given Town NPC is in this state.
        /// </summary>
        public abstract void DoState(TownAIGlobalNPC globalNPC, NPC npc);

        /// <summary>
        /// Retrieves the state integer associated with the type <typeparamref name="T"/>.
        /// </summary>
        public static int GetStateInteger<T>()
            where T : TownNPCAIState => ModContent.GetInstance<T>().ReservedStateInteger;

        protected sealed override void Register() {
            ModTypeLookup<TownNPCAIState>.Register(this);
            Type = StateCount++;
        }
    }
}