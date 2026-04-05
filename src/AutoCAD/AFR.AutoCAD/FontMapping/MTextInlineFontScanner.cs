using Autodesk.AutoCAD.DatabaseServices;

namespace AFR.FontMapping;

/// <summary>
/// 扫描数据库中所有 MText 实体，提取内联字体引用。
/// 遍历所有 BlockTableRecord（含 ModelSpace、PaperSpace 和嵌套块定义），
/// 调用 MTextFontParser 解析每个 MText.Contents。
/// </summary>
internal static class MTextInlineFontScanner
{
    /// <summary>
    /// 扫描数据库中所有 MText 实体的内联字体引用。
    /// 返回去重的字体名 → 字体类型映射。
    /// </summary>
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
