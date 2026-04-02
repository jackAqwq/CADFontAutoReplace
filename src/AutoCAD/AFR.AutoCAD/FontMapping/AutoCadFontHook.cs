using AFR.Abstractions;
using AFR.Models;

namespace AFR.FontMapping;

/// <summary>
/// AutoCAD 字体 Hook 实现（IFontHook 包装器）。
/// 委托给 LdFileHook 静态类执行实际的 ldfile Hook 操作。
/// </summary>
internal sealed class AutoCadFontHook : IFontHook
{
    public bool IsInstalled => LdFileHook.IsInstalled;

    public void Install() => LdFileHook.Install();

    public void Uninstall() => LdFileHook.Uninstall();

    public void UpdateConfig() => LdFileHook.UpdateConfig();

    public List<InlineFontFixRecord> GetRedirectRecords(HashSet<string>? styleTableFontNames = null)
        => LdFileHook.GetRedirectRecords(styleTableFontNames);
}
