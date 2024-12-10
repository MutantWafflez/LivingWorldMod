using ReLogic.Content;

namespace LivingWorldMod.Utilities;

public static partial class LWMUtils {
    public static T ForceLoadAsset<T>(this Asset<T> asset, string modName) where T : class => ModContent.Request<T>($"{modName}/{asset.Name}".Replace("\\", "/"), AssetRequestMode.ImmediateLoad).Value;
}