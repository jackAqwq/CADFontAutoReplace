namespace AFR.Abstractions;

/// <summary>
/// 字体文件加载 Hook 的生命周期管理。
/// AutoCAD 实现 ldfile Hook，其它品牌实现各自机制。
/// </summary>
public interface IFontHook
{
    bool IsInstalled { get; }
    void Install();
    void Uninstall();
    void UpdateConfig();
}
