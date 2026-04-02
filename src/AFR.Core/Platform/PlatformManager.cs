using AFR.Abstractions;

namespace AFR.Platform;

/// <summary>
/// 全局平台管理器，持有当前 CAD 平台的所有服务实例。
/// 由 PluginEntry.Initialize() 在启动时注册。
/// </summary>
public static class PlatformManager
{
    public static ICadPlatform Platform { get; private set; } = null!;
    public static IFontHook FontHook { get; private set; } = null!;
    public static ICadHost Host { get; private set; } = null!;
    public static ILogService? Logger { get; private set; }
    public static IFontScanner? FontScanner { get; private set; }

    public static void Initialize(
        ICadPlatform platform,
        IFontHook fontHook,
        ICadHost host,
        ILogService? logger = null,
        IFontScanner? fontScanner = null)
    {
        Platform = platform ?? throw new ArgumentNullException(nameof(platform));
        FontHook = fontHook ?? throw new ArgumentNullException(nameof(fontHook));
        Host = host ?? throw new ArgumentNullException(nameof(host));
        Logger = logger;
        FontScanner = fontScanner;
    }
}
