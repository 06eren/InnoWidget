using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace InnoWidget.Host;

public partial class WidgetWindow : Window
{
    private readonly SolidColorBrush _backgroundBrush = new(Color.FromRgb(0x0D, 0x0D, 0x0D));

    public WidgetWindow()
    {
        InitializeComponent();
        RootBorder.Background = _backgroundBrush;
        SetBackgroundOpacity(1.0);
        // Ensure widgets stay on desktop (always on bottom)
        Topmost = false;
        ShowInTaskbar = false;
    }

    public void SetBackgroundOpacity(double opacity)
    {
        if (double.IsNaN(opacity) || double.IsInfinity(opacity))
            opacity = 1.0;

        _backgroundBrush.Opacity = Math.Clamp(opacity, 0.2, 1.0);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // Always on bottom: set window to be behind all others
        var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            var exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_NOACTIVATE);
        }
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

internal static class NativeMethods
{
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_NOACTIVATE = 0x08000000;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}
