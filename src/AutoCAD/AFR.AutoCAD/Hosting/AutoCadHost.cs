using AFR.Abstractions;

namespace AFR.Hosting;

/// <summary>
/// AutoCAD 宿主能力实现。
/// </summary>
internal sealed class AutoCadHost : ICadHost
{
    public void ShowModalWindow(object window)
    {
        if (window is System.Windows.Window wpfWindow)
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(wpfWindow);
    }
}
