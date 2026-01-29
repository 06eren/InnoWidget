using System.ComponentModel;
using InnoWidget.Core.Mvvm;
using System.Management;

namespace InnoWidget.Widgets.Battery;

public class BatteryWidgetViewModel : ObservableObject
{
    private int _batteryPercent = 100;
    private string _batteryStatus = "Bilinmiyor";
    private string _timeRemaining = "Bilinmiyor";
    private bool _isCharging = false;

    public BatteryWidgetViewModel()
    {
        LoadBatteryInfo();
        StartTimer();
    }

    public int BatteryPercent
    {
        get => _batteryPercent;
        set => SetProperty(ref _batteryPercent, value);
    }

    public string BatteryStatus
    {
        get => _batteryStatus;
        set => SetProperty(ref _batteryStatus, value);
    }

    public string TimeRemaining
    {
        get => _timeRemaining;
        set => SetProperty(ref _timeRemaining, value);
    }

    public bool IsCharging
    {
        get => _isCharging;
        set => SetProperty(ref _isCharging, value);
    }

    private void LoadBatteryInfo()
    {
        try
        {
            // Simulate battery data for demo
            var rnd = new Random();
            BatteryPercent = rnd.Next(20, 100);
            IsCharging = rnd.Next(0, 3) == 0; // 33% chance of charging
            
            BatteryStatus = IsCharging ? "Şarj Oluyor" : 
                          BatteryPercent > 50 ? "İyi" :
                          BatteryPercent > 20 ? "Orta" : "Düşük";

            var hours = rnd.Next(1, 8);
            var minutes = rnd.Next(0, 60);
            TimeRemaining = IsCharging ? "Şarj Oluyor" : $"{hours}s {minutes}d";
        }
        catch
        {
            BatteryPercent = 75;
            BatteryStatus = "İyi";
            TimeRemaining = "3s 15d";
            IsCharging = false;
        }
    }

    private void StartTimer()
    {
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        timer.Tick += (_, _) => LoadBatteryInfo();
        timer.Start();
    }
}
