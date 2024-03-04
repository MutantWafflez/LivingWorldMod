namespace LivingWorldMod.Content.TownNPCRevitalization.Activities;
/// <summary>
/// With this activity, the given Town NPC will find the nearest pylon and attempt to
/// teleport to another pylon of a biome they like for a brief visit.
/// </summary>
/*
[Autoload(false)]
public class VisitBiomeActivity : TownNPCActivity {
    private static readonly FieldInfo _personalityDatabaseField = typeof(ShopHelper).GetField("_database", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly Dictionary<string, TeleportPylonType> _vanillaBiomeNameToPylon = new() {
        { "Desert", TeleportPylonType.Desert },
        { "Forest", TeleportPylonType.SurfacePurity },
        { "NormalUnderground", TeleportPylonType.Underground },
        { "Mushroom", TeleportPylonType.GlowingMushroom },
        { "Ocean", TeleportPylonType.Beach },
        { "Hallow", TeleportPylonType.Hallow },
        { "Jungle", TeleportPylonType.Jungle }
    };

    private static PersonalityDatabase CurrentDatabase => (PersonalityDatabase)_personalityDatabaseField.GetValue(Main.ShopHelper);

    private Point _destinationPylonPos;
    private byte _activityStage;

    public override TownNPCActivity NewInstance() {
        VisitBiomeActivity activity = (VisitBiomeActivity)base.NewInstance();

        activity._destinationPylonPos = Point16.NegativeOne.ToPoint();
        activity.timeRemaining = int.MaxValue; // TODO: Make an actual good number

        return activity;
    }

    public override bool CanDoActivity(NPC npc) {
        PersonalityDatabase database = CurrentDatabase;
        if (!database.TryGetProfileByNPCID(npc.type, out PersonalityProfile profile)) {
            return false;
        }
        IEnumerable<BiomePreferenceListTrait.BiomePreference> likedBiomes = profile.ShopModifiers
                                                                                   .OfType<BiomePreferenceListTrait>()
                                                                                   .First().Preferences
                                                                                   .Where(preference => preference.Affection > AffectionLevel.Dislike);

        WeightedRandom<Point> possiblePylonLocations = new();
        foreach (BiomePreferenceListTrait.BiomePreference biome in likedBiomes) {
            if (!_vanillaBiomeNameToPylon.TryGetValue(biome.Biome.NameKey, out TeleportPylonType pylonType)) {
                continue;
            }

            if (Main.PylonSystem.Pylons.FirstOrDefault(pylon => pylon.TypeOfPylon == pylonType) is not { PositionInTiles: { X: > 0, Y: > 0 } } pylonInfo) {
                continue;
            }

            Point pylonPos = pylonInfo.PositionInTiles.ToPoint();
            possiblePylonLocations.Add(TileUtils.GetCornerOfMultiTile(Main.tile[pylonPos], pylonPos.X, pylonPos.Y, TileUtils.CornerType.BottomLeft), (double)biome.Affection);
        }

        if (!possiblePylonLocations.elements.Any()) {
            return false;
        }

        _destinationPylonPos = possiblePylonLocations;
        return true;
    }

    public override void DoActivity(TownAIGlobalNPC globalNPC, NPC npc) {
        /*
        if (_activityStage == 0 && npc.velocity.Y == 0f) {
            globalNPC.GenerateNewPath(npc, _destinationPylonPos);

            if (!globalNPC.IsFollowingPath) {
                return;
            }

            _activityStage = 1;
        }

    }
}
*/