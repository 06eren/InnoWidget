using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services;

namespace InnoWidget.Widgets.Heart;

public sealed class HeartWidgetViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _timer;
    private bool _isRefreshing;
    private double _heartRate = 72;
    private double _bloodPressure = 120;
    private double _oxygenLevel = 98;
    private int _pulseCount = 0;

    public double HeartRate
    {
        get => _heartRate;
        private set => SetProperty(ref _heartRate, value);
    }

    public double BloodPressure
    {
        get => _bloodPressure;
        private set => SetProperty(ref _bloodPressure, value);
    }

    public double OxygenLevel
    {
        get => _oxygenLevel;
        private set => SetProperty(ref _oxygenLevel, value);
    }

    public int PulseCount
    {
        get => _pulseCount;
        private set => SetProperty(ref _pulseCount, value);
    }

    public string Title { get; } = "Heart Monitor";

    public bool AnimationsEnabled { get; set; } = true;
    public double AnimationSpeed { get; set; } = 1.0;
    public bool PulseEnabled { get; set; } = true;
    public bool GlowEnabled { get; set; } = true;
    public bool BeatEnabled { get; set; } = true;
    public bool SettingsVisible { get; set; } = false;
    public ICommand ToggleSettingsCommand { get; set; }

    private void ToggleSettings()
    {
        SettingsVisible = !SettingsVisible;
    }

    public HeartWidgetViewModel()
    {
        ToggleSettingsCommand = new RelayCommand(() => ToggleSettings());
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
            // Simulate heart data
            var random = new Random();
            HeartRate = 60 + random.NextDouble() * 40;
            BloodPressure = 100 + random.NextDouble() * 40;
            OxygenLevel = 95 + random.NextDouble() * 5;
            
            // Random pulse
            if (random.NextDouble() > 0.9)
            {
                PulseCount++;
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
