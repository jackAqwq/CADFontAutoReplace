namespace AFR.Abstractions;

/// <summary>
/// 日志服务抽象，供跨层使用。
/// 各 CAD 平台提供具体实现（如 AutoCAD 命令行输出）。
/// </summary>
public interface ILogService
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
    void Error(string message, Exception ex);
}
