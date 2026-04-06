using AFR.Abstractions;

namespace AFR.FontMapping;

/// <summary>
/// AutoCAD 平台的 <see cref="IFontHook"/> 实现。
/// 本身不包含 Hook 逻辑，仅作为接口适配器将调用委托给 <see cref="LdFileHook"/> 静态类。
/// </summary>
internal sealed class AutoCadFontHook : IFontHook
{
    /// <summary>Hook 是否已安装并处于拦截状态。</summary>
    public bool IsInstalled => LdFileHook.IsInstalled;

    /// <summary>安装 ldfile Hook，开始拦截字体文件加载。</summary>
    public void Install() => LdFileHook.Install();

    /// <summary>卸载 Hook，恢复原始 ldfile 函数。</summary>
    public void Uninstall() => LdFileHook.Uninstall();

    /// <summary>更新 Hook 使用的替换字体配置（用户通过 AFR 命令修改后调用）。</summary>
    public void UpdateConfig() => LdFileHook.UpdateConfig();
}
