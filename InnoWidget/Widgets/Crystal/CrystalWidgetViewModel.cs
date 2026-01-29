using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services;

namespace InnoWidget.Widgets.Crystal;

public sealed class CrystalWidgetViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _timer;
    private bool _isRefreshing;
    private double _purity = 95;
    private double _clarity = 88;
    private double _energy = 72;
    private int _vibration = 440;

    public double Purity
    {
        get => _purity;
        private set => SetProperty(ref _purity, value);
    }

    public double Clarity
    {
        get => _clarity;
        private set => SetProperty(ref _clarity, value);
    }

    public double Energy
    {
        get => _energy;
        private set => SetProperty(ref _energy, value);
    }

    public int Vibration
    {
        get => _vibration;
        private set => SetProperty(ref _vibration, value);
    }

    public string Title { get; } = "Crystal Monitor";

    public bool AnimationsEnabled { get; set; } = true;
    public double AnimationSpeed { get; set; } = 1.0;
    public bool PulseEnabled { get; set; } = true;
    public bool GlowEnabled { get; set; } = true;
    public bool ShineEnabled { get; set; } = true;
    public int CrystalDensity { get; set; } = 5;
    public int VibrationFrequency { get; set; } = 440;
    public int MaxEnergyLevel { get; set; } = 100;
    public int HealingThreshold { get; set; } = 85;
    public int SelectedTheme { get; set; } = 0;
    public int BorderStyle { get; set; } = 0;
    public bool SettingsVisible { get; set; } = false;
    public ICommand ToggleSettingsCommand { get; set; }
    public ICommand ResetToDefaultCommand { get; set; }

    private void ToggleSettings()
    {
        SettingsVisible = !SettingsVisible;
    }

    public CrystalWidgetViewModel()
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
        GlowEnabled = true;
        ShineEnabled = true;
        CrystalDensity = 5;
        VibrationFrequency = 440;
        MaxEnergyLevel = 100;
        HealingThreshold = 85;
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
            // Simulate crystal data
            var random = new Random();
            Purity = 85 + random.NextDouble() * 15;
            Clarity = 80 + random.NextDouble() * 20;
            Energy = 60 + random.NextDouble() * 30;
            Vibration = 256 + random.Next(512);
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
