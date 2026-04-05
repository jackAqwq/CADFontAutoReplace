using Microsoft.Win32;
using System.Text.RegularExpressions;
using AFR.Platform;

namespace AFR.Services;

/// <summary>
/// 业务级配置服务，基于注册表存储。
/// 提供带缓存的、类型安全的 MainFont、BigFont、IsInitialized 访问。
/// 通过 PlatformManager 获取注册表路径和应用名称。
/// </summary>
public sealed class ConfigService
{
    private static readonly Lazy<ConfigService> _instance = new(() => new ConfigService());
    public static ConfigService Instance => _instance.Value;

    private static string AutoCadBasePath => PlatformManager.Platform.RegistryBasePath;
    private static string AppName => PlatformManager.Platform.AppName;
    private static string KeyPattern => PlatformManager.Platform.RegistryKeyPattern;

    private Regex? _keyPatternRegex;
    private Regex KeyPatternRegex => _keyPatternRegex ??= new Regex(KeyPattern, RegexOptions.Compiled);

    // 缓存字段
    private string? _mainFont;
    private string? _bigFont;
    private string? _trueTypeFont;
    private int? _isInitialized;
    private volatile bool _cacheLoaded;
    private readonly object _lock = new();

    private List<string>? _resolvedAppPaths;

    private ConfigService() { }

    /// <summary>
    /// 返回所有有效的配置文件应用程序注册表路径。
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
    /// 返回第一个有效的应用程序注册表路径，若未找到则返回 null。
    /// </summary>
    public string? GetPrimaryApplicationPath()
    {
        var paths = GetAllApplicationPaths();
        return paths.Count > 0 ? paths[0] : null;
    }

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
    /// 使内存缓存失效，下次访问时将从注册表重新读取。
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
    /// 删除所有配置文件下的应用注册表项。
    /// 返回成功删除的数量。
    /// </summary>
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
