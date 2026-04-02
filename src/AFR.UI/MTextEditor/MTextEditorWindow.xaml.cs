using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace AFR.UI;

/// <summary>
/// MText 查看器窗口。
/// 显示带语法高亮的 MText 原始代码，只读。
/// 通过 ParentWindowHandle 实现相对于宿主窗口的居中定位。
/// </summary>
public partial class MTextEditorWindow : Window
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    /// <summary>
    /// 宿主窗口句柄，用于居中定位。
    /// 由调用方提供（如 AutoCAD 主窗口句柄）。
    /// </summary>
    public nint ParentWindowHandle { get; set; }

    public MTextEditorWindow(string rawContents)
    {
        InitializeComponent();

        // 在 Loaded 事件中居中，确保在 ShowModalWindow 定位之后执行，
        // 且 PresentationSource 可用于 DPI 换算
        Loaded += (_, _) => CenterOnParentWindow();

        string displayText = MTextEditorViewModel.ToDisplayFormat(rawContents);
        RawViewer.Document = MTextSyntaxHighlighter.CreateHighlightedRawDocument(displayText);
    }

    private void CenterOnParentWindow()
    {
        if (ParentWindowHandle == 0) return;
        if (!GetWindowRect(ParentWindowHandle, out var rect)) return;

        // GetWindowRect 返回屏幕像素，WPF Left/Top 使用逻辑单位（96 DPI 基准）
        // 需要按 DPI 缩放因子换算
        var source = PresentationSource.FromVisual(this);
        double scaleX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        double scaleY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;

        double ownerLeft = rect.Left / scaleX;
        double ownerTop = rect.Top / scaleY;
        double ownerW = (rect.Right - rect.Left) / scaleX;
        double ownerH = (rect.Bottom - rect.Top) / scaleY;

        Left = ownerLeft + (ownerW - ActualWidth) / 2;
        Top = ownerTop + (ownerH - ActualHeight) / 2;
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
