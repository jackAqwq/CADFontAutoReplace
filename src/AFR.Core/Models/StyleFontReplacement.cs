namespace AFR.Models;

/// <summary>
/// 单个样式的字体替换规格。
/// </summary>
public sealed record StyleFontReplacement(
    string StyleName,
    bool IsTrueType,
    string MainFontReplacement,
    string BigFontReplacement);
