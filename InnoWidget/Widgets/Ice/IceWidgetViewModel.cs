using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services;

namespace InnoWidget.Widgets.Ice;

public sealed class IceWidgetViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _timer;
    private bool _isRefreshing;
    private double _temperature = -15;
    private double _iceThickness = 45;
    private double _snowLevel = 78;
    private int _crystalCount = 12;

    public double Temperature
    {
        get => _temperature;
        private set => SetProperty(ref _temperature, value);
    }

    public double IceThickness
    {
        get => _iceThickness;
        private set => SetProperty(ref _iceThickness, value);
    }

    public double SnowLevel
    {
        get => _snowLevel;
        private set => SetProperty(ref _snowLevel, value);
    }

    public int CrystalCount
    {
        get => _crystalCount;
        private set => SetProperty(ref _crystalCount, value);
    }

    public string Title { get; } = "Ice Monitor";

    public bool AnimationsEnabled { get; set; } = true;
    public double AnimationSpeed { get; set; } = 1.0;
    public bool PulseEnabled { get; set; } = true;
    public bool RotateEnabled { get; set; } = true;
    public bool GlowEnabled { get; set; } = true;
    public bool FreezeEnabled { get; set; } = true;
    public bool SnowParticlesEnabled { get; set; } = true;
    public int SnowDensity { get; set; } = 5;
    public double SnowSpeed { get; set; } = 1.0;
    private double _minTemperature;
    public double MinTemperature
    {
        get => _minTemperature;
        private set => SetProperty(ref _minTemperature, value);
    }

    private double _freezingPoint;
    public double FreezingPoint
    {
        get => _freezingPoint;
        private set => SetProperty(ref _freezingPoint, value);
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

    public IceWidgetViewModel()
    {
        ToggleSettingsCommand = new RelayCommand(() => ToggleSettings());
        ResetToDefaultCommand = new RelayCommand(ResetToDefault);
        SettingsVisible = false;

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(3)
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
        FreezeEnabled = true;
        SnowParticlesEnabled = true;
        SnowDensity = 5;
        SnowSpeed = 1.0;
        MinTemperature = -25;
        FreezingPoint = 0;
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
            // Simulate ice data
            var random = new Random();
            Temperature = -25 + random.NextDouble() * 20;
            IceThickness = 30 + random.NextDouble() * 40;
            SnowLevel = 60 + random.NextDouble() * 35;
            
            // Random crystal formation
            if (random.NextDouble() > 0.7)
            {
                CrystalCount++;
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
