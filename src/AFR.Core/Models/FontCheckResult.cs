namespace AFR.Models;

/// <summary>
/// 单个文字样式的字体可用性检查结果。
/// 记录该样式引用了哪些字体文件，以及这些字体在当前环境中是否缺失。
/// </summary>
/// <param name="StyleName">文字样式名称（如 "Standard"、"注释"）。</param>
/// <param name="FileName">主字体文件名（SHX 文件名或 TrueType 字体族名）。</param>
/// <param name="BigFontFileName">大字体文件名（仅 SHX 样式使用，TrueType 样式此值为空）。</param>
/// <param name="IsMainFontMissing">主字体是否缺失（true 表示在可用字体列表中未找到）。</param>
/// <param name="IsBigFontMissing">大字体是否缺失（仅对 SHX 样式有意义）。</param>
/// <param name="IsTrueType">是否为 TrueType 字体样式（false 表示 SHX 字体样式）。</param>
/// <param name="TypeFace">TrueType 字体的字体族名（SHX 样式此值为空）。</param>
public sealed record FontCheckResult(
    string StyleName,
    string FileName,
    string BigFontFileName,
    bool IsMainFontMissing,
    bool IsBigFontMissing,
    bool IsTrueType,
    string TypeFace);
