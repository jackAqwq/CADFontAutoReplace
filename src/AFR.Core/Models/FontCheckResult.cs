namespace AFR.Models;

/// <summary>
/// 单个文字样式的字体可用性检查结果。
/// </summary>
public sealed record FontCheckResult(
    string StyleName,
    string FileName,
    string BigFontFileName,
    bool IsMainFontMissing,
    bool IsBigFontMissing,
    bool IsTrueType,
    string TypeFace);
