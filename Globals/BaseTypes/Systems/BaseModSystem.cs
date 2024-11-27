namespace LivingWorldMod.Globals.BaseTypes.Systems;

/// <summary>
///     ModSystem that all other ModSystems in this mod inherit from; we can shorten ModContent.GetInstance
///     to simply ModSystem.Instance by using this base class.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseModSystem<T> : ModSystem where T : BaseModSystem<T> {
    /// <summary>
    ///     The singleton instance for this ModSystem.
    /// </summary>
    public static T Instance {
        get;
        private set;
    }

    protected BaseModSystem() {
        Instance = (T)this;
    }
}