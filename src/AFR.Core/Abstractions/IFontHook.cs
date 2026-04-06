namespace AFR.Abstractions;

/// <summary>
/// 字体文件加载 Hook 的生命周期管理接口。
/// <para>
/// 通过 Hook CAD 底层的字体加载函数，在字体加载时将缺失字体重定向到替换字体。
/// AutoCAD 平台通过 Hook ldfile 函数实现，其它 CAD 品牌实现各自机制。
/// </para>
/// </summary>
public interface IFontHook
{
    /// <summary>Hook 是否已安装并处于生效状态。</summary>
    bool IsInstalled { get; }
    /// <summary>安装 Hook，开始拦截字体加载请求。</summary>
    void Install();
    /// <summary>卸载 Hook，恢复原始的字体加载行为。</summary>
    void Uninstall();
    /// <summary>在 Hook 已安装的情况下，更新重定向配置（如替换字体映射表变化时调用）。</summary>
    void UpdateConfig();
}
