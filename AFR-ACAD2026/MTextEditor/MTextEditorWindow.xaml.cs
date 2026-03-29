using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AFR_ACAD2026.MTextEditor;

/// <summary>
/// MText 查看器窗口。
/// 显示带语法高亮的 MText 原始代码，只读。
/// </summary>
public partial class MTextEditorWindow : Window
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    internal MTextEditorWindow(string rawContents)
    {
        InitializeComponent();
        CenterOnAcadWindow();

        string displayText = MTextEditorViewModel.ToDisplayFormat(rawContents);
        RawViewer.Document = MTextSyntaxHighlighter.CreateHighlightedRawDocument(displayText);
    }

    private void CenterOnAcadWindow()
    {
        if (!GetWindowRect(AcadApp.MainWindow.Handle, out var rect)) return;

        double ownerW = rect.Right - rect.Left;
        double ownerH = rect.Bottom - rect.Top;
        Left = rect.Left + (ownerW - Width) / 2;
        Top = rect.Top + (ownerH - Height) / 2;
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
