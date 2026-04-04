using System.Collections.Concurrent;
using Autodesk.AutoCAD.DatabaseServices;

namespace AFR.Services;

/// <summary>
/// 单次字体检测/替换事务的执行上下文。
/// 封装 Database 引用和所有缓存，生命周期与单次 Execute 事务绑定。
/// 事务结束后由 GC 自动回收，不同图纸之间实现 100% 内存隔离。
/// </summary>
public sealed class FontDetectionContext
{
    public Database Db { get; }

    /// <summary>FindFile 结果缓存。Key: "{hint}:{normalizedFileName}" Value: 是否找到</summary>
    public ConcurrentDictionary<string, bool> FindFileCache { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>SHX 文件类型分类缓存。Key: filePath Value: isBigFont</summary>
    public ConcurrentDictionary<string, bool> ShxTypeCache { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>TrueType 字体度量缓存。Key: fontName Value: (CharacterSet, PitchAndFamily)</summary>
    public ConcurrentDictionary<string, (int CharacterSet, int PitchAndFamily)> FontMetricsCache { get; }
        = new(StringComparer.OrdinalIgnoreCase);

    public FontDetectionContext(Database db)
    {
        Db = db ?? throw new ArgumentNullException(nameof(db));
    }
}
