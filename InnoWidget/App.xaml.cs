using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;
using InnoWidget.Host;
using InnoWidget.Core.Services;
using InnoWidget.Core.Services.Media;
using InnoWidget.Shell;
using InnoWidget.Widgets.Hardware;
using InnoWidget.Widgets.Media;
using InnoWidget.Widgets.Network;
using InnoWidget.Widgets.Notes;
using InnoWidget.Widgets.World;

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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _cpuRamService = new CpuRamMonitoringService();
            var networkService = new NetworkMonitoringService();
            _httpClient = new HttpClient();
            _weatherService = new OpenMeteoWeatherService(_httpClient);
            _mediaService = new GsmTcMediaSessionService();

            _loadedSettings = _layoutStore.Load();

            _definitions = new List<WidgetDefinition>
            {
                new WidgetDefinition(
                    id: "hardware",
                    title: "Mini Donanım Monitörü",
                    defaultSize: new Size(280, 180),
                    createViewModel: () =>
                    {
                        var vm = new HardwareWidgetViewModel(_cpuRamService);
                        _disposables.Add(vm);
                        return vm;
                    }),

                new WidgetDefinition(
                    id: "network",
                    title: "Ağ Trafiği İzleyici",
                    defaultSize: new Size(280, 180),
                    createViewModel: () =>
                    {
                        var vm = new NetworkWidgetViewModel(networkService);
                        _disposables.Add(vm);
                        return vm;
                    }),

                new WidgetDefinition(
                    id: "notes",
                    title: "Hızlı Not / To-Do",
                    defaultSize: new Size(280, 220),
                    createViewModel: () => new NotesWidgetViewModel()),

                new WidgetDefinition(
                    id: "world",
                    title: "Dünya Saatleri ve Hava Durumu",
                    defaultSize: new Size(560, 220),
                    createViewModel: () =>
                    {
                        var vm = new WorldClockWidgetViewModel(_weatherService!);
                        _disposables.Add(vm);
                        return vm;
                    }),

                new WidgetDefinition(
                    id: "media",
                    title: "Spotify/Medya Kontrolcü",
                    defaultSize: new Size(320, 220),
                    createViewModel: () =>
                    {
                        var vm = new MediaWidgetViewModel(_mediaService!);
                        _disposables.Add(vm);
                        return vm;
                    })
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

            foreach (var def in _definitions)
            {
                var settings = GetOrCreateSettings(def);
                if (settings.IsOpen)
                    _widgetHost.Show(def, settings);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            PersistLayout();
            _widgetHost.CloseAll();

            for (var i = _disposables.Count - 1; i >= 0; i--)
                _disposables[i].Dispose();

            _cpuRamService?.Dispose();
            _httpClient?.Dispose();
            _mediaService?.Dispose();
            base.OnExit(e);
        }

        private WidgetSettings GetOrCreateSettings(WidgetDefinition def)
        {
            if (_loadedSettings.TryGetValue(def.Id, out var existing))
                return existing;

            return new WidgetSettings
            {
                Id = def.Id,
                IsOpen = false,
                Left = 100,
                Top = 100,
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
