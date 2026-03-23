using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AFR_ACAD2026.Services;

namespace AFR_ACAD2026.UI;

/// <summary>
/// 字体替换日志中的单行条目（每个缺失字体一行）。
/// 将"样式+主字体缺失"和"样式+大字体缺失"拆分为独立行，
/// 使表格布局紧凑统一。
/// </summary>
internal sealed class FontReplacementRow : INotifyPropertyChanged
{
    private string _selectedReplacement = string.Empty;

    public string StyleName { get; }
    public string FontCategory { get; }
    public string MissingFontName { get; }
    public bool IsTrueType { get; }
    public bool IsBigFont { get; }

    /// <summary>可供选择的替换字体列表（根据字体类型自动匹配）。</summary>
    public ObservableCollection<string> AvailableFonts { get; }

    public string SelectedReplacement
    {
        get => _selectedReplacement;
        set
        {
            if (_selectedReplacement == value) return;
            _selectedReplacement = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public FontReplacementRow(
        string styleName,
        string fontCategory,
        string missingFontName,
        bool isTrueType,
        bool isBigFont,
        ObservableCollection<string> availableFonts,
        string autoReplacement)
    {
        StyleName = styleName;
        FontCategory = fontCategory;
        MissingFontName = missingFontName;
        IsTrueType = isTrueType;
        IsBigFont = isBigFont;
        AvailableFonts = availableFonts;
        _selectedReplacement = autoReplacement;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// 字体替换日志窗口的 ViewModel。
/// 将检测结果拆平为每个缺失字体一行，按类型分组排序（SHX → TrueType → 大字体）。
/// </summary>
internal sealed class FontReplacementLogViewModel : INotifyPropertyChanged
{
    public ObservableCollection<FontReplacementRow> Items { get; } = [];
    public string SummaryText { get; }
    public int ShxCount { get; }
    public int TrueTypeCount { get; }
    public int BigFontCount { get; }
    public string ShxLabel => $"SHX  {ShxCount}";
    public string TrueTypeLabel => $"TrueType  {TrueTypeCount}";
    public string BigFontLabel => $"大字体  {BigFontCount}";
    public bool HasShx => ShxCount > 0;
    public bool HasTrueType => TrueTypeCount > 0;
    public bool HasBigFont => BigFontCount > 0;
    public bool HasItems => Items.Count > 0;
    public bool HasNoItems => !HasItems;

    public FontReplacementLogViewModel(
        IReadOnlyList<FontCheckResult>? detectionResults,
        string globalMainFont,
        string globalBigFont,
        string globalTrueTypeFont)
    {
        var shxFonts = new ObservableCollection<string>(FontSelectionViewModel.ScanAvailableFonts());
        var ttFonts = new ObservableCollection<string>(FontSelectionViewModel.ScanSystemTrueTypeFonts());

        if (detectionResults != null && detectionResults.Count > 0)
        {
            int ttCount = 0, shxCount = 0, bigCount = 0;
            var shxRows = new List<FontReplacementRow>();
            var ttRows = new List<FontReplacementRow>();
            var bigRows = new List<FontReplacementRow>();

            foreach (var r in detectionResults)
            {
                if (r.IsMainFontMissing)
                {
                    string category = r.IsTrueType ? "TrueType" : "SHX";
                    string missingName = r.IsTrueType
                        ? (!string.IsNullOrEmpty(r.TypeFace) ? r.TypeFace : r.FileName)
                        : r.FileName;
                    string autoReplacement = r.IsTrueType ? globalTrueTypeFont : globalMainFont;
                    var fonts = r.IsTrueType ? ttFonts : shxFonts;

                    var row = new FontReplacementRow(
                        r.StyleName, category, missingName,
                        r.IsTrueType, false, fonts, autoReplacement);

                    if (r.IsTrueType) { ttRows.Add(row); ttCount++; }
                    else { shxRows.Add(row); shxCount++; }
                }

                if (r.IsBigFontMissing)
                {
                    bigRows.Add(new FontReplacementRow(
                        r.StyleName, "大字体", r.BigFontFileName,
                        false, true, shxFonts, globalBigFont));
                    bigCount++;
                }
            }

            // 按类型分组排序：SHX → TrueType → 大字体
            foreach (var row in shxRows) Items.Add(row);
            foreach (var row in ttRows) Items.Add(row);
            foreach (var row in bigRows) Items.Add(row);

            ShxCount = shxCount;
            TrueTypeCount = ttCount;
            BigFontCount = bigCount;

            int total = ttCount + shxCount + bigCount;
            SummaryText = $"{detectionResults.Count} 个样式 · {total} 个缺失";
        }
        else
        {
            SummaryText = "未检测到缺失字体";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
