using System.Windows.Controls;
using System.Windows.Input;

namespace InnoWidget.Widgets.Notes;

public partial class NotesWidgetView : UserControl
{
    public NotesWidgetView()
    {
        InitializeComponent();
    }

    private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBox tb)
            return;

        if (tb.IsReadOnly)
        {
            tb.IsReadOnly = false;
            tb.Focus();
            e.Handled = true;
        }
    }

    private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox tb)
            tb.IsReadOnly = true;
    }
}
