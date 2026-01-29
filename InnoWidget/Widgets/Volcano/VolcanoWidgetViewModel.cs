using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services;

namespace InnoWidget.Widgets.Volcano;

public sealed class VolcanoWidgetViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _timer;
    private bool _isRefreshing;
    private double _temperature = 850;
    private double _pressure = 2.5;
    private double _lavaLevel = 75;
    private int _eruptionCount = 3;

    public double Temperature
    {
        get => _temperature;
        private set => SetProperty(ref _temperature, value);
    }

    public double Pressure
    {
        get => _pressure;
        private set => SetProperty(ref _pressure, value);
    }

    public double LavaLevel
    {
        get => _lavaLevel;
        private set => SetProperty(ref _lavaLevel, value);
    }

    public int EruptionCount
    {
        get => _eruptionCount;
        private set => SetProperty(ref _eruptionCount, value);
    }

    public string Title { get; } = "Volcano Monitor";

    public bool AnimationsEnabled { get; set; } = true;
    public double AnimationSpeed { get; set; } = 1.0;
    public bool PulseEnabled { get; set; } = true;
    public bool RotateEnabled { get; set; } = true;
    public bool GlowEnabled { get; set; } = true;
    public bool LavaParticlesEnabled { get; set; } = true;
    public int EruptionFrequency { get; set; } = 5;
    public int LavaDensity { get; set; } = 5;
    private double _maxTemperature;
    public double MaxTemperature
    {
        get => _maxTemperature;
        private set => SetProperty(ref _maxTemperature, value);
    }

    private double _alertThreshold;
    public double AlertThreshold
    {
        get => _alertThreshold;
        private set => SetProperty(ref _alertThreshold, value);
    }
    public int SelectedTheme { get; set; } = 0;
    public int BorderStyle { get; set; } = 0;
    public bool SettingsVisible { get; set; } = false;
    public ICommand ToggleSettingsCommand { get; set; }
    public ICommand ResetToDefaultCommand { get; set; }

    private void ToggleSettings()
    {
        SettingsVisible = !SettingsVisible;
    }

    public VolcanoWidgetViewModel()
    {
        ToggleSettingsCommand = new RelayCommand(() => ToggleSettings());
        ResetToDefaultCommand = new RelayCommand(ResetToDefault);
        SettingsVisible = false;

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(2)
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
        LavaParticlesEnabled = true;
        EruptionFrequency = 5;
        LavaDensity = 5;
        MaxTemperature = 1200;
        AlertThreshold = 1000;
        SelectedTheme = 0;
        BorderStyle = 0;
    }

    private async Task RefreshAsync()
    {
        if (_isRefreshing)
            return;

        _isRefreshing = true;
        try
        {
            // Simulate volcano data
            var random = new Random();
            Temperature = 800 + random.NextDouble() * 200;
            Pressure = 1.5 + random.NextDouble() * 2;
            LavaLevel = 60 + random.NextDouble() * 35;
            
            // Random eruptions
            if (random.NextDouble() > 0.8)
            {
                EruptionCount++;
            }
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
