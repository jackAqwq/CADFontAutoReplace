namespace AFR.Abstractions;

/// <summary>
/// 字体扫描抽象，返回当前 CAD 环境可用的字体列表。
/// 各 CAD 平台根据自身的搜索路径和字体目录提供实现。
/// </summary>
public interface IFontScanner
{
    /// <summary>扫描 CAD 搜索路径下的可用 SHX 字体文件名。</summary>
    IReadOnlyCollection<string> ScanAvailableShxFonts();

    /// <summary>扫描系统已安装的 TrueType 字体族名。</summary>
    IReadOnlyCollection<string> ScanSystemTrueTypeFonts();
}
