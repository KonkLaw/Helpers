using System.Windows;

namespace Notifier;

/// <summary>
/// Interaction logic for ErrorWindow.xaml
/// </summary>
public partial class ErrorWindow : Window
{
    protected ErrorWindow()
    {
        InitializeComponent();
    }

    public ErrorWindow(string? text)
    {
        InitializeComponent();
        TextBox.Text = text;
    }
}