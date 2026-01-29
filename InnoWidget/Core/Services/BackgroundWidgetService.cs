using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using InnoWidget.Host;
using Timer = System.Threading.Timer;

namespace InnoWidget.Core.Services;

public class BackgroundWidgetService : IDisposable
{
    private readonly WidgetHostService _widgetHost;
    private readonly List<WidgetWindow> _backgroundWidgets = new();
    private readonly Timer _backgroundTimer;
    private bool _isBackgroundMode;
    private bool _disposed;

    public bool IsBackgroundMode 
    { 
        get => _isBackgroundMode; 
        private set => _isBackgroundMode = value; 
    }

    public BackgroundWidgetService(WidgetHostService widgetHost)
    {
        _widgetHost = widgetHost;
        
        // Her 30 saniyede bir widget'ları kontrol et
        _backgroundTimer = new Timer(CheckBackgroundWidgets, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public void EnableBackgroundMode()
    {
        if (_isBackgroundMode) return;

        _isBackgroundMode = true;
        
        // Mevcut tüm widget'ları arkaplana taşı
        var activeWidgets = _widgetHost.GetAllActiveWidgets().ToList();
        
        foreach (var widget in activeWidgets)
        {
            MoveToBackground(widget);
        }

        // Ana pencereyi gizle
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
                Application.Current.MainWindow.Hide();
            }
        });
    }

    public void DisableBackgroundMode()
    {
        if (!_isBackgroundMode) return;

        _isBackgroundMode = false;
        
        // Tüm arkaplan widget'larını geri getir
        var widgetsToRestore = _backgroundWidgets.ToList();
        
        foreach (var widget in widgetsToRestore)
        {
            RestoreFromBackground(widget);
        }

        // Ana pencereyi göster
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        });
    }

    private void MoveToBackground(WidgetWindow widget)
    {
        if (_backgroundWidgets.Contains(widget)) return;

        _backgroundWidgets.Add(widget);
        
        // Widget'ı arkaplan moduna ayarla
        Application.Current.Dispatcher.Invoke(() =>
        {
            widget.ShowInTaskbar = false;
            widget.Topmost = false;
            widget.WindowState = WindowState.Minimized;
            widget.Hide();
        });
    }

    private void RestoreFromBackground(WidgetWindow widget)
    {
        if (!_backgroundWidgets.Contains(widget)) return;

        _backgroundWidgets.Remove(widget);
        
        // Widget'ı normal moda geri döndür
        Application.Current.Dispatcher.Invoke(() =>
        {
            widget.ShowInTaskbar = true;
            widget.WindowState = WindowState.Normal;
            widget.Show();
        });
    }

    private void CheckBackgroundWidgets(object? state)
    {
        if (!_isBackgroundMode || _disposed) return;

        try
        {
            // Arkaplan widget'larının durumunu kontrol et
            Application.Current.Dispatcher.Invoke(() =>
            {
                var widgetsToRemove = new List<WidgetWindow>();
                
                foreach (var widget in _backgroundWidgets)
                {
                    if (!widget.IsLoaded)
                    {
                        widgetsToRemove.Add(widget);
                    }
                }

                // Kapalı widget'ları listeden çıkar
                foreach (var widget in widgetsToRemove)
                {
                    _backgroundWidgets.Remove(widget);
                }
            });
        }
        catch (Exception ex)
        {
            // Hata loglama
            System.Diagnostics.Debug.WriteLine($"Background widget check error: {ex.Message}");
        }
    }

    public void ToggleBackgroundMode()
    {
        if (_isBackgroundMode)
        {
            DisableBackgroundMode();
        }
        else
        {
            EnableBackgroundMode();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _backgroundTimer?.Dispose();
        
        // Tüm widget'ları normal moda geri döndür
        if (_isBackgroundMode)
        {
            DisableBackgroundMode();
        }
    }
}
