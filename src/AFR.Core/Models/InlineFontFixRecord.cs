namespace AFR.Models;

/// <summary>
/// 单条 MText 字体映射记录。
/// </summary>
public sealed record InlineFontFixRecord(
    string MissingFont,
    string ReplacementFont,
    string FixMethod,
    string FontCategory);
