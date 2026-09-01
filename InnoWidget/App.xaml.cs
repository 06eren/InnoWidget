using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;
using InnoWidget.Core.Services;
using InnoWidget.Host;
using InnoWidget.Shell;
using InnoWidget.Widgets.Hardware;
using InnoWidget.Widgets.Network;
using InnoWidget.Widgets.Notes;
using InnoWidget.Widgets.World;
using InnoWidget.Widgets.Media;
using InnoWidget.Widgets.SystemInfo;
using InnoWidget.Widgets.Battery;
using InnoWidget.Core.Services.Media;
using InnoWidget.Widgets.Weather;
using InnoWidget.Widgets.Disk;
using InnoWidget.Widgets.ProcessMonitor;
using InnoWidget.Widgets.Temperature;
using InnoWidget.Widgets.Test;
using InnoWidget.Widgets.Volcano;
using InnoWidget.Widgets.Ice;
using InnoWidget.Widgets.Crystal;
using InnoWidget.Widgets.Heart;
using System.Diagnostics;
using System.Windows.Threading;
using System.Text.Json;
using System.IO;
using InnoWidget.Core.Mvvm;

namespace InnoWidget
{
    public partial class App : Application
    {
        private CpuRamMonitoringService? _cpuRamService;
        private HttpClient? _httpClient;
        private OpenMeteoWeatherService? _weatherService;
        private IMediaSessionService? _mediaService;
        private readonly List<IDisposable> _disposables = new();
        private readonly WidgetLayoutStore _layoutStore = new();
        private readonly WidgetHostService _widgetHost = new();
        private IReadOnlyDictionary<string, WidgetSettings> _loadedSettings = new Dictionary<string, WidgetSettings>(StringComparer.OrdinalIgnoreCase);
        private List<WidgetDefinition> _definitions = new();
        
        // Background ve System Tray servisleri
        private BackgroundWidgetService? _backgroundService;
        private SimpleSystemTrayService? _systemTrayService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Enable native performance optimizations (disabled for now)
            // PerformanceOptimizer.Instance.OptimizeApplication();

            _cpuRamService = new CpuRamMonitoringService();
            var networkService = new NetworkMonitoringService();
            _httpClient = new HttpClient();
            _weatherService = new OpenMeteoWeatherService(_httpClient);
            _mediaService = new GsmTcMediaSessionService();

            _loadedSettings = _layoutStore.Load();

            _definitions = new List<WidgetDefinition>
            {
                // BASİT TEST WIDGET'I - KASMA SORUNU TEST
                new WidgetDefinition(
                    id: "test",
                    title: "Test Widget",
                    defaultSize: new Size(280, 120),
                    createViewModel: () => new TestWidgetViewModel()),

                // Essential widget'lar - minimalist ve şık (timer'lar kapalı)
                // Hardware geçici olarak devre dışı - binding sorunu var
                /*
                new WidgetDefinition(
                    id: "hardware",
                    title: "System Monitor",
                    defaultSize: new Size(280, 180),
                    createViewModel: () =>
                    {
                        var vm = new HardwareWidgetViewModel(_cpuRamService);
                        _disposables.Add(vm);
                        return vm;
                    }),
                */

                new WidgetDefinition(
                    id: "weather",
                    title: "Weather",
                    defaultSize: new Size(280, 120),
                    createViewModel: () =>
                    {
                        var vm = new WeatherWidgetViewModel(_weatherService);
                        _disposables.Add(vm);
                        return vm;
                    }),

                new WidgetDefinition(
                    id: "notes",
                    title: "Quick Notes",
                    defaultSize: new Size(280, 150),
                    createViewModel: () => new NotesWidgetViewModel()),
            };

            var toggles = _definitions.Select(def =>
            {
                var settings = GetOrCreateSettings(def);
                var iconUri = def.Id.ToLowerInvariant() switch
                {
                    "hardware" => "pack://application:,,,/Assets/Icons/cpu.svg",
                    "network" => "pack://application:,,,/Assets/Icons/network.svg",
                    "notes" => "pack://application:,,,/Assets/Icons/note.svg",
                    "world" => "pack://application:,,,/Assets/Icons/world.svg",
                    "media" => "pack://application:,,,/Assets/Icons/media.svg",
                    "system" => "pack://application:,,,/Assets/Icons/system.svg",
                    "battery" => "pack://application:,,,/Assets/Icons/battery.svg",
                    "weather" => "pack://application:,,,/Assets/Icons/cloud.svg",
                    "disk" => "pack://application:,,,/Assets/Icons/disk.svg",
                    "process" => "pack://application:,,,/Assets/Icons/process.svg",
                    "temperature" => "pack://application:,,,/Assets/Icons/thermometer.svg",
                    _ => "pack://application:,,,/Assets/Icons/widget.svg"
                };

                return new WidgetToggleItemViewModel(def.Id, def.Title, iconUri, settings.IsOpen, settings.Opacity, isOpen =>
                {
                    settings.IsOpen = isOpen;
                    if (isOpen)
                        _widgetHost.Show(def, settings);
                    else
                        _widgetHost.Close(def.Id);

                    PersistLayout();
                }, opacity =>
                {
                    settings.Opacity = opacity;
                    _widgetHost.SetOpacity(def.Id, opacity);
                    PersistLayout();
                });
            }).ToArray();

            var shellVm = new ShellViewModel(toggles);

            var window = new MainWindow
            {
                DataContext = shellVm
            };

            MainWindow = window;
            window.Show();

            // TEST: Widget'ı direkt göster
            Console.WriteLine("[APP] TEST: Creating test widget directly");
            var testDef = _definitions.First(d => d.Id == "test");
            var testSettings = new WidgetSettings
            {
                Id = "test",
                IsOpen = true,
                Left = 500,
                Top = 300,
                Width = 280,
                Height = 120,
                Opacity = 1.0
            };
            _widgetHost.Show(testDef, testSettings);
            Console.WriteLine("[APP] TEST: Test widget shown");

            foreach (var def in _definitions)
            {
                var settings = GetOrCreateSettings(def);
                System.Diagnostics.Debug.WriteLine($"[APP] Processing widget: {def.Id}, IsOpen: {settings.IsOpen}");
                if (settings.IsOpen)
                {
                    System.Diagnostics.Debug.WriteLine($"[APP] Showing widget: {def.Id}");
                    _widgetHost.Show(def, settings);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[APP] Skipping widget: {def.Id} (IsOpen = false)");
                }
            }

            // Background ve System Tray servislerini başlat
            _backgroundService = new BackgroundWidgetService(_widgetHost);
            _systemTrayService = new SimpleSystemTrayService(_backgroundService);
            _disposables.Add(_backgroundService);
            _disposables.Add(_systemTrayService);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                PersistLayout();
                _widgetHost.CloseAll();

                // Restore performance settings (disabled for now)
                // PerformanceOptimizer.Instance.RestoreApplication();

                // Background ve System Tray servislerini temizle
                _backgroundService?.Dispose();
                _systemTrayService?.Dispose();

                // Tüm disposable'ları temizle (reverse order)
                for (var i = _disposables.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        _disposables[i]?.Dispose();
                    }
                    catch
                    {
                        // Dispose sırasında hata olursa sessizce geç
                    }
                }

                // Service'leri temizle
                _cpuRamService?.Dispose();
                _httpClient?.Dispose();
                _mediaService?.Dispose();
                // _weatherService?.Dispose(); // OpenMeteoWeatherService Dispose implement etmiyor
            }
            catch
            {
                // Exit sırasında hata olursa sessizce geç
            }
            finally
            {
                base.OnExit(e);
            }
        }

        private WidgetSettings GetOrCreateSettings(WidgetDefinition def)
        {
            if (_loadedSettings.TryGetValue(def.Id, out var existing))
                return existing;

            // Tüm widget'ları varsayılan olarak açık yap
            bool isOpen = true;  // TÜM WIDGET'LAR AÇIK

            return new WidgetSettings
            {
                Id = def.Id,
                IsOpen = isOpen,
                Left = 100 + (_definitions.IndexOf(def) * 320),  // Yan yana diz
                Top = 100 + (_definitions.IndexOf(def) * 200),  // Alt alta diz
                Width = def.DefaultSize.Width,
                Height = def.DefaultSize.Height,
                Opacity = 1.0
            };
        }

        private void PersistLayout()
        {
            var open = _widgetHost.CaptureLayout().ToDictionary(x => x.Id, x => x, StringComparer.OrdinalIgnoreCase);
            var all = new List<WidgetSettings>();

            foreach (var def in _definitions)
            {
                var s = GetOrCreateSettings(def);
                if (open.TryGetValue(def.Id, out var opened))
                {
                    opened.IsOpen = true;
                    opened.Opacity = s.Opacity;
                    all.Add(opened);
                }
                else
                {
                    all.Add(s);
                }
            }

            _loadedSettings = all.ToDictionary(x => x.Id, x => x, StringComparer.OrdinalIgnoreCase);
            _layoutStore.Save(all);
        }
    }

}
