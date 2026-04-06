using Autodesk.AutoCAD.DatabaseServices;

namespace AFR.FontMapping;

/// <summary>
/// 扫描 AutoCAD 数据库中所有 MText（多行文字）实体，提取其内联字体引用。
/// <para>
/// 遍历所有 BlockTableRecord（包括 ModelSpace、PaperSpace 和嵌套块定义），
/// 对每个 MText 实体调用 <see cref="MTextFontParser.ParseInlineFonts"/> 解析其 Contents 属性。
/// 无法访问的实体或块表记录会被静默跳过。
/// </para>
/// </summary>
internal static class MTextInlineFontScanner
{
    /// <summary>
    /// 扫描数据库中所有 MText 实体的内联字体引用。
    /// </summary>
    /// <param name="db">要扫描的 AutoCAD 数据库。</param>
    /// <returns>去重的字体名 → 字体类型映射（不区分大小写）。</returns>
    internal static Dictionary<string, InlineFontType> ScanInlineFonts(Database db)
    {
        var result = new Dictionary<string, InlineFontType>(StringComparer.OrdinalIgnoreCase);

        using var tr = db.TransactionManager.StartOpenCloseTransaction();
        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

        foreach (ObjectId btrId in bt)
        {
            try
            {
                var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);

                foreach (ObjectId entId in btr)
                {
                    try
                    {
                        if (tr.GetObject(entId, OpenMode.ForRead) is MText mtext)
                        {
                            MTextFontParser.ParseInlineFonts(mtext.Contents, result);
                        }
                    }
                    catch
                    {
                        // 跳过无法访问的实体
                    }
                }
            }
            catch
            {
                // 跳过无法访问的块表记录
            }
        }

        tr.Commit();
        return result;
    }
}
