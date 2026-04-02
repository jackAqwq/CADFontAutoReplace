namespace AFR.Abstractions;

/// <summary>
/// CAD 平台身份标识与版本特定常量。
/// 每个版本适配壳提供唯一实现。
/// </summary>
public interface ICadPlatform
{
    // ── 身份 ──
    string BrandName { get; }
    string VersionName { get; }
    string AppName { get; }
    string DisplayName { get; }

    // ── 注册表 ──
    string RegistryBasePath { get; }
    string RegistryKeyPattern { get; }

    // ── Hook 参数 ──
    string AcDbDllName { get; }
    string LdFileExport { get; }
    int PrologueSize { get; }

    // ── 能力标记 ──
    bool SupportsLdFileHook { get; }
}
