using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services;

namespace InnoWidget.Widgets.Hardware;

public sealed class HardwareWidgetViewModel : ObservableObject, IDisposable
{
    private readonly IMonitoringService<CpuRamSnapshot> _service;
    private readonly DispatcherTimer _timer;
    private bool _isRefreshing;

    private double _cpuPercent;
    public double CpuPercent
    {
        get => _cpuPercent;
        private set => SetProperty(ref _cpuPercent, value);
    }

    private double _ramPercent;
    public double RamPercent
    {
        get => _ramPercent;
        private set => SetProperty(ref _ramPercent, value);
    }

    public string Title { get; } = "Mini Donanım Monitörü";

    public bool AnimationsEnabled { get; set; } = true;
    public double AnimationSpeed { get; set; } = 1.0;
    public bool PulseEnabled { get; set; } = true;
    public bool RotateEnabled { get; set; } = true;
    public bool GlowEnabled { get; set; } = false;
    public bool SakuraEnabled { get; set; } = true;
    public int PetalDensity { get; set; } = 5;
    public double PetalSpeed { get; set; } = 1.0;
    public int UpdateInterval { get; set; } = 2000;
    public bool HighPerformanceMode { get; set; } = true;
    public bool NativeOptimization { get; set; } = true;
    public int SelectedTheme { get; set; } = 0;
    public int BorderStyle { get; set; } = 0;
    public bool SettingsVisible { get; set; } = false;
    public ICommand ToggleSettingsCommand { get; set; }
    public ICommand ResetToDefaultCommand { get; set; }

    private void ToggleSettings()
    {
        SettingsVisible = !SettingsVisible;
    }

    public HardwareWidgetViewModel(IMonitoringService<CpuRamSnapshot> service)
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

    private async Task RefreshAsync()
    {
        if (_isRefreshing)
            return;

        _isRefreshing = true;
        try
        {
            var snap = await Task.Run(() => _service.GetSnapshot()).ConfigureAwait(true);
            CpuPercent = snap.CpuPercent;
            RamPercent = snap.RamPercent;
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void ResetToDefault()
    {
        AnimationsEnabled = true;
        AnimationSpeed = 1.0;
        PulseEnabled = true;
        RotateEnabled = true;
        GlowEnabled = false;
        SakuraEnabled = true;
        PetalDensity = 5;
        PetalSpeed = 1.0;
        UpdateInterval = 2000;
        HighPerformanceMode = true;
        NativeOptimization = true;
        SelectedTheme = 0;
        BorderStyle = 0;
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
