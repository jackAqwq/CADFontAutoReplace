namespace AFR.Abstractions;

/// <summary>
/// CAD 宿主环境的通用操作抽象。
/// 用于解耦 AFR.UI 对特定 CAD API 的直接依赖。
/// </summary>
public interface ICadHost
{
    void ShowModalWindow(object window);
}
