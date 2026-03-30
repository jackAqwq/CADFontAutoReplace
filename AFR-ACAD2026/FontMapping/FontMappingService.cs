using AFR_ACAD2026.Services;

namespace AFR_ACAD2026.FontMapping;

/// <summary>
/// 单条内联字体修复记录。
/// </summary>
internal sealed record InlineFontFixRecord(
    string MissingFont,
    string ReplacementFont,
    string FixMethod,      // "Hook重定向"
    string FontCategory);  // "SHX主字体" / "SHX大字体" / "TrueType"

/// <summary>
/// 字体映射服务 — 通过 ldfile Hook 处理 MText 内联字体缺失。
///
/// Hook 在 PluginEntry.Initialize() 时安装，拦截 acdb25.dll 的 ldfile 函数。
/// 当 AutoCAD 在 DWG 解析阶段加载字体文件时，Hook 检测缺失字体并
/// 透明重定向到用户配置的替换字体，无需创建外部文件。
///
/// 本类提供 Execute() 阶段的查询接口，收集 Hook 的重定向记录供 AFRLOG 显示。
/// </summary>
internal static class FontMappingService
{
    /// <summary>
    /// 收集 Hook 已完成的重定向记录，供 AFRLOG 界面显示。
    /// </summary>
    internal static List<InlineFontFixRecord> CollectRedirectRecords()
    {
        return LdFileHook.GetRedirectRecords();
    }
}
