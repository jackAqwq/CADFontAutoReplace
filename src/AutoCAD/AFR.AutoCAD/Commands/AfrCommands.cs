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
            log.Info("AFR 命令已调用。");

            var window = new FontSelectionWindow();
            PlatformManager.Host.ShowModalWindow(window);

            if (window.DialogResult != true)
            {
                log.Info("AFR 命令已被用户取消。");
                return;
            }

            // 保存配置
            var config = ConfigService.Instance;
            config.MainFont = window.SelectedMainFont;
            config.BigFont = window.SelectedBigFont;
            config.TrueTypeFont = window.SelectedTrueTypeFont;
            config.IsInitialized = true;

            log.Info($"配置已保存 — 主字体: '{window.SelectedMainFont}', TrueType字体: '{window.SelectedTrueTypeFont}', 大字体: '{window.SelectedBigFont}'");

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
            log.Error("AFR 命令执行失败", ex);
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
                log.Info("请先打开一个图纸文件。");
                return;
            }

            List<FontCheckResult>? results;
            Dictionary<string, (string FileName, string BigFontFileName, string TypeFace)>? currentFonts = null;

            using (doc.LockDocument())
            {
                // 始终从数据库重新检测缺失字体
                // 用户可能通过 ST 命令手工修改了字体，缓存的检测结果已过时
                FontDetector.ClearCaches();
                results = FontDetector.DetectMissingFonts(doc.Database);

                // 更新存储的检测结果，保持一致性
                DocumentContextManager.Instance.StoreDetectionResults(doc, results);

                // 读取图纸中各样式的当前实际字体（反映手动替换/ST命令修改后的状态）
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
                    int count = FontReplacer.ReplaceByStyleMapping(doc.Database, replacements);
                    if (count > 0) doc.Editor.Regen();
                    return count;
                }
            };
            PlatformManager.Host.ShowModalWindow(window);

            if (window.AppliedCount > 0)
            {
                log.Info($"手动替换完成 — 共替换 {window.AppliedCount} 个样式的字体。");
            }
        }
        catch (System.Exception ex)
        {
            log.Error("AFRLOG 命令执行失败", ex);
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
        var editor = AcadApp.DocumentManager.MdiActiveDocument?.Editor;
        var log = LogService.Instance;

        try
        {
            log.Info("正在执行 AFRUNLOAD — 卸载 AFR 插件...");

            // 第一步：注销事件、清空队列和文档跟踪
            PluginEntryBase.Unload();
            log.Info("已注销所有文档事件监听。");
            log.Info("已清空执行队列和文档跟踪状态。");

            // 第二步：删除注册表项（仅 AFR-ACAD2026）
            var config = ConfigService.Instance;
            int deletedCount = config.DeleteAllApplicationKeys();
            log.Info($"注册表清理完成 — 共删除 {deletedCount} 个 AFR-ACAD2026 注册表项。");

            log.Info("AFR 插件已完全卸载。");
            log.Info("如需重新加载，请重启CAD后使用 NETLOAD 命令加载新路径下的 DLL。");

            // 第三步：先输出日志，再完成卸载
            log.Flush();

            // 最终确认（直接输出，因为 Flush 已完成）
            editor?.WriteMessage("\nAFR 插件已卸载完成，可通过 NETLOAD 重新加载。\n");
        }
        catch (System.Exception ex)
        {
            log.Error("AFRUNLOAD 命令执行失败", ex);
            log.Flush();
            editor?.WriteMessage($"\n卸载失败: {ex.Message}\n");
        }
    }
}
