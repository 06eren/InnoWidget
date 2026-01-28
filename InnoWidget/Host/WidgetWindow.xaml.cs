using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace InnoWidget.Host;

public partial class WidgetWindow : Window
{
    private readonly SolidColorBrush _backgroundBrush = new(Color.FromRgb(0x12, 0x12, 0x12));

    public WidgetWindow()
    {
        InitializeComponent();
        RootBorder.Background = _backgroundBrush;
        SetBackgroundOpacity(1.0);
    }

    public void SetBackgroundOpacity(double opacity)
    {
        if (double.IsNaN(opacity) || double.IsInfinity(opacity))
            opacity = 1.0;

        _backgroundBrush.Opacity = Math.Clamp(opacity, 0.2, 1.0);
    }

    private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            Topmost = !Topmost;
            return;
        }

        DragMove();
    }
}
