using AFR.Abstractions;

namespace AFR.Platform;

/// <summary>
/// 全局平台管理器，作为服务定位器持有当前 CAD 平台的所有核心服务实例。
/// <para>
/// 由各 CAD 版本适配壳（如 PluginEntry）在插件启动时调用 <see cref="Initialize"/> 注册服务。
/// 其它层通过此类的静态属性访问平台相关服务，避免直接依赖具体实现。
/// </para>
/// </summary>
public static class PlatformManager
{
    /// <summary>当前 CAD 平台的身份标识与版本常量。</summary>
    public static ICadPlatform Platform { get; private set; } = null!;
    /// <summary>字体文件加载 Hook 服务。</summary>
    public static IFontHook FontHook { get; private set; } = null!;
    /// <summary>CAD 宿主环境操作服务（如模态窗口显示）。</summary>
    public static ICadHost Host { get; private set; } = null!;
    /// <summary>日志服务（可选，某些测试环境下可能为 null）。</summary>
    public static ILogService? Logger { get; private set; }
    /// <summary>字体扫描服务（可选）。</summary>
    public static IFontScanner? FontScanner { get; private set; }

    /// <summary>
    /// 注册所有平台服务实例。必须在插件启动时调用一次。
    /// </summary>
    /// <param name="platform">CAD 平台身份标识（必需）。</param>
    /// <param name="fontHook">字体 Hook 服务（必需）。</param>
    /// <param name="host">CAD 宿主环境操作服务（必需）。</param>
    /// <param name="logger">日志服务（可选）。</param>
    /// <param name="fontScanner">字体扫描服务（可选）。</param>
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
