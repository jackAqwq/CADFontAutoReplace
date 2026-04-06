using Microsoft.Win32;
using System.Text.RegularExpressions;
using AFR.Platform;

namespace AFR.Services;

/// <summary>
/// 业务级配置服务，使用 Windows 注册表作为持久化存储。
/// <para>
/// 采用全局单例模式，通过 <see cref="Instance"/> 访问。
/// 提供带内存缓存的、类型安全的配置项访问（MainFont、BigFont、TrueTypeFont、IsInitialized）。
/// 配置通过 <see cref="PlatformManager"/> 获取注册表路径和应用名称，
/// 写入时会同步到所有匹配的 CAD 版本注册表项中。
/// </para>
/// </summary>
public sealed class ConfigService
{
    // 使用 Lazy<T> 实现线程安全的延迟初始化单例
    private static readonly Lazy<ConfigService> _instance = new(() => new ConfigService());
    /// <summary>获取 ConfigService 的全局唯一实例。</summary>
    public static ConfigService Instance => _instance.Value;

    // 从 PlatformManager 获取当前 CAD 平台的注册表配置路径信息
    private static string AutoCadBasePath => PlatformManager.Platform.RegistryBasePath;
    private static string AppName => PlatformManager.Platform.AppName;
    private static string KeyPattern => PlatformManager.Platform.RegistryKeyPattern;

    // 编译后的正则表达式，用于匹配注册表中属于当前 CAD 版本的子键名
    private Regex? _keyPatternRegex;
    private Regex KeyPatternRegex => _keyPatternRegex ??= new Regex(KeyPattern, RegexOptions.Compiled);

    // ── 内存缓存字段：避免每次访问都读注册表 ──
    private string? _mainFont;
    private string? _bigFont;
    private string? _trueTypeFont;
    private int? _isInitialized;
    // volatile 确保多线程环境下读取到最新值
    private volatile bool _cacheLoaded;
    private readonly object _lock = new();

    // 缓存解析后的所有有效注册表路径
    private List<string>? _resolvedAppPaths;

    // 私有构造函数：强制使用 Instance 单例访问
    private ConfigService() { }

    /// <summary>
    /// 返回所有匹配当前 CAD 版本的应用程序注册表路径。
    /// <para>
    /// 遍历注册表中 CAD 根路径下的所有子键，筛选出与版本模式匹配的子键，
    /// 然后拼接为完整的 Applications\{AppName} 路径。结果会被缓存。
    /// </para>
    /// </summary>
    public IReadOnlyList<string> GetAllApplicationPaths()
    {
        var cached = _resolvedAppPaths;
        if (cached != null) return cached;

        lock (_lock)
        {
            if (_resolvedAppPaths != null) return _resolvedAppPaths;

            var results = new List<string>();
            var subKeyNames = RegistryService.GetSubKeyNames(Registry.CurrentUser, AutoCadBasePath);
            foreach (var name in subKeyNames)
            {
                if (KeyPatternRegex.IsMatch(name))
                {
                    results.Add($@"{AutoCadBasePath}\{name}\Applications\{AppName}");
                }
            }

            _resolvedAppPaths = results;
            return results;
        }
    }

    /// <summary>
    /// 返回第一个匹配的应用程序注册表路径，用于读写配置值。
    /// 若未找到任何匹配路径则返回 null。
    /// </summary>
    public string? GetPrimaryApplicationPath()
    {
        var paths = GetAllApplicationPaths();
        return paths.Count > 0 ? paths[0] : null;
    }

    /// <summary>
    /// 确保内存缓存已从注册表加载（双重检查锁定模式）。
    /// 首次调用时从注册表读取所有配置值，后续调用直接跳过。
    /// </summary>
    private void EnsureCacheLoaded()
    {
        if (_cacheLoaded) return;
        lock (_lock)
        {
            if (_cacheLoaded) return;
            var path = GetPrimaryApplicationPath();
            if (path == null) return;

            _mainFont = RegistryService.ReadString(Registry.CurrentUser, path, "MainFont");
            _bigFont = RegistryService.ReadString(Registry.CurrentUser, path, "BigFont");
            _trueTypeFont = RegistryService.ReadString(Registry.CurrentUser, path, "TrueTypeFont");
            _isInitialized = RegistryService.ReadDword(Registry.CurrentUser, path, "IsInitialized");
            _cacheLoaded = true;
        }
    }

    /// <summary>
    /// SHX 主字体替换名称。
    /// 读取时从缓存获取（首次自动加载），写入时同步到所有匹配的注册表路径。
    /// </summary>
    public string MainFont
    {
        get
        {
            EnsureCacheLoaded();
            return _mainFont ?? string.Empty;
        }
        set
        {
            foreach (var path in GetAllApplicationPaths())
            {
                RegistryService.WriteString(Registry.CurrentUser, path, "MainFont", value);
            }
            lock (_lock) { _mainFont = value; }
        }
    }

    /// <summary>
    /// SHX 大字体替换名称。
    /// 读取时从缓存获取，写入时同步到所有匹配的注册表路径。
    /// </summary>
    public string BigFont
    {
        get
        {
            EnsureCacheLoaded();
            return _bigFont ?? string.Empty;
        }
        set
        {
            foreach (var path in GetAllApplicationPaths())
            {
                RegistryService.WriteString(Registry.CurrentUser, path, "BigFont", value);
            }
            lock (_lock) { _bigFont = value; }
        }
    }

    /// <summary>
    /// TrueType 字体替换名称。
    /// 读取时从缓存获取，写入时同步到所有匹配的注册表路径。
    /// </summary>
    public string TrueTypeFont
    {
        get
        {
            EnsureCacheLoaded();
            return _trueTypeFont ?? string.Empty;
        }
        set
        {
            foreach (var path in GetAllApplicationPaths())
            {
                RegistryService.WriteString(Registry.CurrentUser, path, "TrueTypeFont", value);
            }
            lock (_lock) { _trueTypeFont = value; }
        }
    }

    /// <summary>
    /// 插件是否已完成首次初始化配置。
    /// 注册表中以 DWORD 值存储（1=已初始化，0=未初始化）。
    /// </summary>
    public bool IsInitialized
    {
        get
        {
            EnsureCacheLoaded();
            return (_isInitialized ?? 0) == 1;
        }
        set
        {
            int val = value ? 1 : 0;
            foreach (var path in GetAllApplicationPaths())
            {
                RegistryService.WriteDword(Registry.CurrentUser, path, "IsInitialized", val);
            }
            lock (_lock) { _isInitialized = val; }
        }
    }

    /// <summary>
    /// 使内存缓存失效，下次访问配置属性时将从注册表重新读取。
    /// 通常在外部修改了注册表或需要刷新配置时调用。
    /// </summary>
    public void InvalidateCache()
    {
        lock (_lock)
        {
            _cacheLoaded = false;
            _mainFont = null;
            _bigFont = null;
            _trueTypeFont = null;
            _isInitialized = null;
            _resolvedAppPaths = null;
        }
    }

    /// <summary>
    /// 删除所有匹配的 CAD 版本注册表中本插件的应用键。
    /// 用于插件卸载时清理注册表。
    /// </summary>
    /// <returns>成功删除的注册表键数量。</returns>
    public int DeleteAllApplicationKeys()
    {
        int deletedCount = 0;

        var subKeyNames = RegistryService.GetSubKeyNames(Registry.CurrentUser, AutoCadBasePath);
        foreach (var name in subKeyNames)
        {
            if (!KeyPatternRegex.IsMatch(name)) continue;

            var appKeyPath = $@"{AutoCadBasePath}\{name}\Applications\{AppName}";

            if (!RegistryService.KeyExists(Registry.CurrentUser, appKeyPath)) continue;

            if (RegistryService.DeleteSubKeyTree(Registry.CurrentUser, appKeyPath))
            {
                deletedCount++;
            }
        }

        InvalidateCache();
        return deletedCount;
    }
}
