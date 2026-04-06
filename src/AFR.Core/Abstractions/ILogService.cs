namespace AFR.Abstractions;

/// <summary>
/// 日志服务抽象接口，定义了统一的日志写入方法。
/// <para>
/// 本接口供 Core / UI 等跨层代码使用，不依赖具体 CAD 平台。
/// 各 CAD 平台（如 AutoCAD）负责提供实际实现，将日志输出到对应的命令行或面板。
/// </para>
/// </summary>
public interface ILogService
{
    /// <summary>记录一条信息级别日志。</summary>
    /// <param name="message">日志内容。</param>
    void Info(string message);
    /// <summary>记录一条警告级别日志。</summary>
    /// <param name="message">日志内容。</param>
    void Warning(string message);
    /// <summary>记录一条错误级别日志。</summary>
    /// <param name="message">日志内容。</param>
    void Error(string message);
    /// <summary>记录一条包含异常详情的错误级别日志。</summary>
    /// <param name="message">错误描述。</param>
    /// <param name="ex">关联的异常对象。</param>
    void Error(string message, Exception ex);
}
