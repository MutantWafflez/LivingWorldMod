using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.Configs;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds.Pyramid.PyramidGenTasks {
    public class CurseGenerationPyramidTask : PyramidSubworld.PyramidGenerationTask {
        public override string StepName => "Room Curse Generation";

        public override void DoTask(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Cursing the Path";

            //Generate torches in all rooms
            List<List<PyramidRoom>> allPaths = PyramidSubworld.fakePaths.Prepend(PyramidSubworld.correctPath).ToList();
            for (int i = 0; i < allPaths.Count; i++) {
                progress.Set(i / (allPaths.Count - 1f));

                GenerateRoomCurseOnPath(allPaths[i]);
            }
        }

        /// <summary>
        /// If a room is cursed, generate its curse's effect on the generation of the room,
        /// if applicable.
        /// </summary>
        private void GenerateRoomCurseOnPath(List<PyramidRoom> path) {
            for (int i = 0; i < path.Count; i++) {
                PyramidRoom room = path[i];

                if (room.generationStep >= PyramidSubworld.PyramidRoomGenerationStep.CurseGenerated) {
                    continue;
                }
                room.generationStep = PyramidSubworld.PyramidRoomGenerationStep.CurseGenerated;

                if (room.roomType != PyramidRoomType.Cursed) {
                    continue;
                }

                if (LivingWorldMod.IsDebug && ModContent.GetInstance<DebugConfig>().allCursedRooms) {
                    room.ActiveCurses.Add(ModContent.GetInstance<DebugConfig>().forcedCurseType);
                }
                else {
                    room.AddRandomCurse(false);
                }
                room.ApplyOneTimeCurseEffects();
            }
        }
    }
}