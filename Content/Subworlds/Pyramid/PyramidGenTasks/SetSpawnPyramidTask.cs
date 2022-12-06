using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.Subworlds.Pyramid.PyramidGenTasks {
    public class SetSpawnPyramidTask : PyramidSubworld.PyramidGenerationTask {
        public override string StepName => "Set Spawn";

        public override void DoTask(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Spawn Point";

            Main.spawnTileX = PyramidSubworld.spawnTileX;
            Main.spawnTileY = PyramidSubworld.spawnTileY;
        }
    }
}