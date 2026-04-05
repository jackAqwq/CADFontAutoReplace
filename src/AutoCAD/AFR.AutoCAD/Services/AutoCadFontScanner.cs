using System.Diagnostics;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media;
using AFR.Abstractions;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AFR.Services;

/// <summary>
/// AutoCAD 平台的字体扫描实现。
/// 扫描 AutoCAD 搜索路径下的 SHX 字体和系统 TrueType 字体。
/// 使用会话级缓存 — 字体列表在 CAD 运行期间不变，只扫描一次。
/// </summary>
internal sealed class AutoCadFontScanner : IFontScanner
{
    private static SortedSet<string>? _cachedShxFonts;
    private static SortedSet<string>? _cachedTrueTypeFonts;

    public IReadOnlyCollection<string> ScanAvailableShxFonts()
    {
        if (_cachedShxFonts != null) return _cachedShxFonts;

        var fonts = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        // 扫描 AutoCAD 支持搜索路径 (ACADPREFIX)
        try
        {
            var prefix = (string)AcadApp.GetSystemVariable("ACADPREFIX");
            if (!string.IsNullOrEmpty(prefix))
            {
                foreach (var dir in prefix.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    ScanDirectory(dir.Trim(), "*.shx", fonts);
                }
            }
        }
        catch { }

        // 扫描 AutoCAD 安装目录 Fonts 文件夹
        try
        {
            var processPath = Environment.ProcessPath
                ?? Process.GetCurrentProcess().MainModule?.FileName;
            if (processPath != null)
            {
                var fontsDir = Path.Combine(Path.GetDirectoryName(processPath)!, "Fonts");
                ScanDirectory(fontsDir, "*.shx", fonts);
            }
        }
        catch { }

        _cachedShxFonts = fonts;
        return fonts;
    }

    public IReadOnlyCollection<string> ScanSystemTrueTypeFonts()
    {
        if (_cachedTrueTypeFonts != null) return _cachedTrueTypeFonts;

        var fonts = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var zhCN = XmlLanguage.GetLanguage("zh-cn");
            var zh = XmlLanguage.GetLanguage("zh");

            foreach (var family in Fonts.SystemFontFamilies)
            {
                string? displayName = null;

                if (family.FamilyNames.TryGetValue(zhCN, out var zhCNName)
                    && !string.IsNullOrWhiteSpace(zhCNName))
                {
                    displayName = zhCNName;
                }
                else if (family.FamilyNames.TryGetValue(zh, out var zhName)
                         && !string.IsNullOrWhiteSpace(zhName))
                {
                    displayName = zhName;
                }

                if (displayName == null) continue;
                if (HasInvalidChars(displayName)) continue;
                if (!ValidateFontName(family, displayName)) continue;

                fonts.Add(displayName);
            }
        }
        catch { }
        _cachedTrueTypeFonts = fonts;
        return fonts;
    }

    private static bool ValidateFontName(FontFamily family, string displayName)
    {
        try
        {
            var typeface = new Typeface(family, System.Windows.FontStyles.Normal, System.Windows.FontWeights.Normal, System.Windows.FontStretches.Normal);
            if (!typeface.TryGetGlyphTypeface(out var glyph))
                return true;

            string? filePath = glyph.FontUri?.LocalPath;
            if (string.IsNullOrEmpty(filePath)) return true;

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrEmpty(fileName)) return true;

            if (!ContainsCjk(fileName) || !ContainsCjk(displayName))
                return true;

            foreach (char c in displayName)
            {
                if (c >= '\u4E00' && c <= '\u9FFF' && fileName.Contains(c))
                    return true;
            }

            return false;
        }
        catch
        {
            return true;
        }
    }

    private static bool ContainsCjk(string s)
    {
        foreach (char c in s)
        {
            if (c >= '\u4E00' && c <= '\u9FFF') return true;
        }
        return false;
    }

    private static bool HasInvalidChars(string name)
    {
        foreach (char c in name)
        {
            if (char.IsDigit(c)) continue;
            if (char.IsLetter(c))
            {
                if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.ModifierLetter)
                    return true;
                continue;
            }
            if (c is ' ' or '-' or '_' or '(' or ')' or '（' or '）' or '.' or '·') continue;
            return true;
        }
        return false;
    }

    private static void ScanDirectory(string directory, string pattern, SortedSet<string> results)
    {
        if (!Directory.Exists(directory)) return;
        try
        {
            foreach (var file in Directory.EnumerateFiles(directory, pattern))
            {
                var name = Path.GetFileName(file);
                if (!string.IsNullOrEmpty(name))
                    results.Add(name);
            }
        }
        catch { }
    }
}
