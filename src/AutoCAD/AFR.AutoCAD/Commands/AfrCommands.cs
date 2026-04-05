using Autodesk.AutoCAD.Runtime;
using AFR.Hosting;
using AFR.FontMapping;
using AFR.Models;
using AFR.Platform;
using AFR.Services;
using AFR.UI;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AFR.Commands;

/// <summary>
/// AFR 插件的 AutoCAD 命令定义。
/// </summary>
public class AfrCommands
{
    /// <summary>
    /// AFR 命令: 打开字体配置界面，保存设置，
    /// 设置 IsInitialized = 1，并对当前文档执行字体替换。
    /// </summary>
    [CommandMethod("AFR")]
    public void AfrCommand()
    {
        var log = LogService.Instance;
        try
        {
            var window = new FontSelectionWindow();
            PlatformManager.Host.ShowModalWindow(window);

            if (window.DialogResult != true)
            {
                return;
            }

            // 保存配置
            var config = ConfigService.Instance;
            config.MainFont = window.SelectedMainFont;
            config.BigFont = window.SelectedBigFont;
            config.TrueTypeFont = window.SelectedTrueTypeFont;
            config.IsInitialized = true;

            // 更新 Hook 的替换字体配置
            PlatformManager.FontHook.UpdateConfig();

            // 对当前文档执行字体替换
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                // 重置执行跟踪，以便重新处理当前文档
                DocumentContextManager.Instance.Remove(doc);
                ExecutionController.Instance.Execute(doc, "AFR Command");
            }
        }
        catch (System.Exception ex)
        {
            log.Error("配置保存失败", ex);
        }
        finally
        {
            log.Flush();
        }
    }

    /// <summary>
    /// AFRLOG 命令: 打开字体替换日志界面。
    /// 显示缺失字体检测结果，支持手动逐一指定替换字体（仅影响当前图纸，不写入注册表）。
    /// 每次打开时重新检测，反映 ST 命令等外部修改后的最新状态。
    /// </summary>
    [CommandMethod("AFRLOG")]
    public void AfrLogCommand()
    {
        var log = LogService.Instance;
        try
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                log.Info("请先打开图纸。");
                return;
            }

            List<FontCheckResult>? results;
            Dictionary<string, (string FileName, string BigFontFileName, string TypeFace)>? currentFonts = null;

            using (doc.LockDocument())
            {
                // 创建独立的执行上下文 — 每次 AFRLOG 命令使用全新缓存
                var context = new FontDetectionContext(doc.Database);

                // 从数据库重新检测当前缺失字体
                results = FontDetector.DetectMissingFonts(context);

                if (results.Count > 0)
                {
                    // 仍有缺失字体 → 更新存储结果，反映最新状态
                    DocumentContextManager.Instance.StoreDetectionResults(doc, results);
                }
                else
                {
                    // 当前无缺失 → 尝试使用自动替换阶段存储的检测结果
                    // 这样用户可以看到哪些字体被自动替换过
                    var stored = DocumentContextManager.Instance.GetDetectionResults(doc);
                    if (stored != null && stored.Count > 0)
                        results = stored;
                }

                // 读取图纸中各样式的当前实际字体（反映替换/ST命令修改后的状态）
                if (results.Count > 0)
                {
                    currentFonts = FontDetector.ReadCurrentFontAssignments(doc.Database);
                }
            }

            var config = ConfigService.Instance;
            var inlineFixResults = DocumentContextManager.Instance.GetInlineFontFixResults(doc);
            var vm = new FontReplacementLogViewModel(
                results, config.MainFont, config.BigFont, config.TrueTypeFont, currentFonts, inlineFixResults);

            var window = new FontReplacementLogWindow(vm);
            window.ApplyReplacementsHandler = replacements =>
            {
                using (doc.LockDocument())
                {
                    // 手动替换也使用独立上下文
                    var replaceContext = new FontDetectionContext(doc.Database);
                    int count = FontReplacer.ReplaceByStyleMapping(replacements, replaceContext);
                    if (count > 0) doc.Editor.Regen();
                    return count;
                }
            };
            PlatformManager.Host.ShowModalWindow(window);

            if (window.AppliedCount > 0)
            {
                log.Info($"已替换 {window.AppliedCount} 个样式的字体。");
            }
        }
        catch (System.Exception ex)
        {
            log.Error("日志查看失败", ex);
        }
        finally
        {
            log.Flush();
        }
    }

    /// <summary>
    /// AFRUNLOAD 命令: 完整卸载插件。
    /// 注销所有事件监听、删除 AFR-ACAD2026 注册表项、清空运行状态。
    /// 卸载后插件不再自动运行，用户可从其他路径重新加载。
    /// </summary>
    [CommandMethod("AFRUNLOAD")]
    public void AfrUnloadCommand()
    {
        var log = LogService.Instance;

        try
        {
            // 第一步：注销事件、清空队列和文档跟踪
            PluginEntryBase.Unload();

            // 第二步：删除注册表项（仅 AFR-ACAD2026）
            var config = ConfigService.Instance;
            config.DeleteAllApplicationKeys();

            log.Info("AFR 已卸载，重启 CAD 后可通过 NETLOAD 重新加载。");

            // 输出日志
            log.Flush();
        }
        catch (System.Exception ex)
        {
            log.Error("卸载失败", ex);
            log.Flush();
        }
    }
}
