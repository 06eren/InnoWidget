using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using InnoWidget.Core.Formatting;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services;

namespace InnoWidget.Widgets.Network;

public sealed class NetworkWidgetViewModel : ObservableObject, IDisposable
{
    private readonly IMonitoringService<NetworkSnapshot> _service;
    private readonly DispatcherTimer _timer;
    private bool _isRefreshing;

    private const int HistorySize = 60;
    private readonly double[] _downloadHistory = new double[HistorySize];
    private readonly double[] _uploadHistory = new double[HistorySize];
    private int _historyIndex;

    public IReadOnlyList<double> DownloadHistory => _downloadHistory;
    public IReadOnlyList<double> UploadHistory => _uploadHistory;

    private double _downloadBitsPerSecond;
    public double DownloadBitsPerSecond
    {
        get => _downloadBitsPerSecond;
        private set
        {
            if (SetProperty(ref _downloadBitsPerSecond, value))
                OnPropertyChanged(nameof(DownloadText));
        }
    }

    private double _uploadBitsPerSecond;
    public double UploadBitsPerSecond
    {
        get => _uploadBitsPerSecond;
        private set
        {
            if (SetProperty(ref _uploadBitsPerSecond, value))
                OnPropertyChanged(nameof(UploadText));
        }
    }

    public string DownloadText => RateFormatter.BitsPerSecondToString(DownloadBitsPerSecond);
    public string UploadText => RateFormatter.BitsPerSecondToString(UploadBitsPerSecond);

    public string Title { get; } = "Ağ Trafiği İzleyici";

    public bool AnimationsEnabled { get; set; } = true;
    public double AnimationSpeed { get; set; } = 1.0;
    public bool PulseEnabled { get; set; } = true;
    public bool RotateEnabled { get; set; } = true;
    public bool GlowEnabled { get; set; } = true;
    public bool NeonParticlesEnabled { get; set; } = true;
    public int NeonIntensity { get; set; } = 5;
    public int GlowRadius { get; set; } = 15;
    public int UpdateInterval { get; set; } = 1000;
    public bool HighSpeedMode { get; set; } = true;
    public bool RealTimeUpdates { get; set; } = true;
    public int SelectedNeonColor { get; set; } = 0;
    public int BackgroundStyle { get; set; } = 0;
    public bool SettingsVisible { get; set; } = false;
    public ICommand ToggleSettingsCommand { get; set; }
    public ICommand ResetToDefaultCommand { get; set; }

    private void ToggleSettings()
    {
        SettingsVisible = !SettingsVisible;
    }

    public NetworkWidgetViewModel(IMonitoringService<NetworkSnapshot> service)
    {
        _service = service;
        ToggleSettingsCommand = new RelayCommand(() => ToggleSettings());
        ResetToDefaultCommand = new RelayCommand(ResetToDefault);
        SettingsVisible = false;

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += async (_, _) => await RefreshAsync().ConfigureAwait(true);

        _ = RefreshAsync();
        _timer.Start();
    }

    private void ResetToDefault()
    {
        AnimationsEnabled = true;
        AnimationSpeed = 1.0;
        PulseEnabled = true;
        RotateEnabled = true;
        GlowEnabled = true;
        NeonParticlesEnabled = true;
        NeonIntensity = 5;
        GlowRadius = 15;
        UpdateInterval = 1000;
        HighSpeedMode = true;
        RealTimeUpdates = true;
        SelectedNeonColor = 0;
        BackgroundStyle = 0;
    }

    private async Task RefreshAsync()
    {
        if (_isRefreshing)
            return;

        _isRefreshing = true;
        try
        {
            var snap = await Task.Run(() => _service.GetSnapshot()).ConfigureAwait(true);
            DownloadBitsPerSecond = snap.DownloadBitsPerSecond;
            UploadBitsPerSecond = snap.UploadBitsPerSecond;

            _downloadHistory[_historyIndex] = DownloadBitsPerSecond;
            _uploadHistory[_historyIndex] = UploadBitsPerSecond;
            _historyIndex = (_historyIndex + 1) % HistorySize;

            OnPropertyChanged(nameof(DownloadHistory));
            OnPropertyChanged(nameof(UploadHistory));
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
