using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace InnoWidget.Core.Services;

public class SimpleSystemTrayService : IDisposable
{
    private readonly BackgroundWidgetService _backgroundService;
    private bool _disposed;
    private IntPtr _trayIconHandle;
    private const int WM_TRAYICON = 0x0400;
    private const int NIM_ADD = 0x00000000;
    private const int NIM_DELETE = 0x00000002;
    private const int NIM_MODIFY = 0x00000001;
    private const int NIF_MESSAGE = 0x00000001;
    private const int NIF_ICON = 0x00000002;
    private const int NIF_TIP = 0x00000004;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;

    [StructLayout(LayoutKind.Sequential)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA pnid);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public SimpleSystemTrayService(BackgroundWidgetService backgroundService)
    {
        _backgroundService = backgroundService;
        
        // Basit bir tray icon oluştur
        CreateTrayIcon();
    }

    private void CreateTrayIcon()
    {
        var nid = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
            hWnd = new IntPtr(0), // Basit implementasyon için
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = IntPtr.Zero, // Basit icon
            szTip = "InnoWidget - Widget Manager"
        };

        Shell_NotifyIcon(NIM_ADD, ref nid);
    }

    public void ToggleBackgroundMode()
    {
        _backgroundService.ToggleBackgroundMode();
        
        // Notification göster
        MessageBox.Show(
            _backgroundService.IsBackgroundMode ? "Arkaplan modu aktif" : "Arkaplan modu pasif",
            "Widget'lar arkaplanda çalışıyor",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        
        // Tray icon'u kaldır
        var nid = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
            hWnd = new IntPtr(0),
            uID = 1
        };
        
        Shell_NotifyIcon(NIM_DELETE, ref nid);
    }
}
