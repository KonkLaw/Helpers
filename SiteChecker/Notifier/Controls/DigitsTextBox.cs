using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Notifier.Controls;

public class DigitsTextBox : TextBox
{
    private static readonly Regex Regex = new Regex("^[0-9]");

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        if (!Regex.IsMatch(e.Text))
            e.Handled = true;
        base.OnPreviewTextInput(e);
    }
}