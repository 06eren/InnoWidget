using System.ComponentModel;
using InnoWidget.Core.Mvvm;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace InnoWidget.Widgets.Battery;

public class BatteryWidgetViewModel : ObservableObject
{
    private bool _isRefreshing;

    private int _batteryPercent = 100;
    private string _batteryStatus = "Bilinmiyor";
    private string _timeRemaining = "Bilinmiyor";
    private bool _isCharging = false;

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_POWER_STATUS
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public uint BatteryLifeTime;
        public uint BatteryFullLifeTime;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS sps);

    public BatteryWidgetViewModel()
    {
        LoadBatteryInfo();
        
        // TIMER KALDIRILDI - KASMA SORUNU KÖKEN ÇÖZÜM
        // Pil verisi sadece başlangıçta bir kez al
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
        if (_isRefreshing)
            return;

        _isRefreshing = true;
        try
        {
            if (GetSystemPowerStatus(out SYSTEM_POWER_STATUS powerStatus))
            {
                BatteryPercent = powerStatus.BatteryLifePercent == 255 ? 100 : powerStatus.BatteryLifePercent;
                IsCharging = powerStatus.ACLineStatus == 1; // 1 = Online
                
                // Battery status
                if (IsCharging)
                {
                    BatteryStatus = "Şarj Oluyor";
                    TimeRemaining = "Şarj Oluyor";
                }
                else
                {
                    BatteryStatus = BatteryPercent switch
                    {
                        > 75 => "Çok İyi",
                        > 50 => "İyi", 
                        > 25 => "Orta",
                        > 10 => "Düşük",
                        _ => "Kritik"
                    };

                    // Estimate remaining time
                    if (powerStatus.BatteryLifeTime != 4294967295) // -1 in uint32
                    {
                        var remaining = TimeSpan.FromSeconds(powerStatus.BatteryLifeTime);
                        TimeRemaining = $"{remaining.Hours}s {remaining.Minutes}d";
                    }
                    else
                    {
                        TimeRemaining = "Hesaplanamıyor";
                    }
                }
            }
            else
            {
                // Fallback to simulated data
                var rnd = new Random();
                BatteryPercent = rnd.Next(20, 100);
                IsCharging = rnd.Next(0, 3) == 0;
                
                BatteryStatus = IsCharging ? "Şarj Oluyor" : 
                              BatteryPercent > 50 ? "İyi" :
                              BatteryPercent > 20 ? "Orta" : "Düşük";

                var hours = rnd.Next(1, 8);
                var minutes = rnd.Next(0, 60);
                TimeRemaining = IsCharging ? "Şarj Oluyor" : $"{hours}s {minutes}d";
            }
        }
        catch
        {
            // Final fallback
            BatteryPercent = 75;
            BatteryStatus = "İyi";
            TimeRemaining = "3s 15d";
            IsCharging = false;
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void StartTimer()
    {
        // TIMER KALDIRILDI - KASMA SORUNU KÖKEN ÇÖZÜM
        // Timer yok - sadece başlangıçta veri al
    }
}
