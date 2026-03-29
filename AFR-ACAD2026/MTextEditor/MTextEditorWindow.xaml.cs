using System.Windows;
using System.Windows.Input;

namespace AFR_ACAD2026.MTextEditor;

/// <summary>
/// MText 查看器窗口。
/// 显示带语法高亮的 MText 原始代码，只读。
/// </summary>
public partial class MTextEditorWindow : Window
{
    internal MTextEditorWindow(string rawContents)
    {
        InitializeComponent();

        string displayText = MTextEditorViewModel.ToDisplayFormat(rawContents);
        RawViewer.Document = MTextSyntaxHighlighter.CreateHighlightedRawDocument(displayText);
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
