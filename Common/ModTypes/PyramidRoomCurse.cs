using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.ModTypes {
    public abstract class PyramidRoomCurse : ModType {
        /// <summary>
        /// The numerical type of this Curse. Auto-assigned at initialization.
        /// </summary>
        public int CurseType {
            get;
            private set;
        }

        /// <summary>
        /// The count of all PyramidRoomCurse extensions/objects that exist.
        /// </summary>
        public static int CurseTypeCount {
            get;
            private set;
        }

        /// <summary>
        /// Gets the numerical type of this Curse.
        /// </summary>
        public static int GetCurseType<T>() where T : PyramidRoomCurse => ModContent.GetInstance<T>().CurseType;

        /// <summary>
        /// Is called once when the curse is first applied. Use this to generate
        /// something in the room.
        /// </summary>
        /// <param name="roomRegion"></param>
        public virtual void DoGenerationEffect(Rectangle roomRegion) { }

        protected override void Register() {
            ModTypeLookup<PyramidRoomCurse>.Register(this);
            CurseType = CurseTypeCount;
            CurseTypeCount++;
        }

        public sealed override void SetupContent() => SetStaticDefaults();
    }
}