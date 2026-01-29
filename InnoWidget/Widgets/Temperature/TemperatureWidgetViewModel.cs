using System.ComponentModel;
using InnoWidget.Core.Mvvm;
using System.Management;

namespace InnoWidget.Widgets.Temperature;

public class TemperatureWidgetViewModel : ObservableObject
{
    private string _cpuTemp = "45°C";
    private string _gpuTemp = "55°C";
    private string _systemTemp = "38°C";
    private string _status = "Normal";

    public TemperatureWidgetViewModel()
    {
        LoadTemperatureInfo();
        StartTimer();
    }

    public string CpuTemp
    {
        get => _cpuTemp;
        set => SetProperty(ref _cpuTemp, value);
    }

    public string GpuTemp
    {
        get => _gpuTemp;
        set => SetProperty(ref _gpuTemp, value);
    }

    public string SystemTemp
    {
        get => _systemTemp;
        set => SetProperty(ref _systemTemp, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    private void LoadTemperatureInfo()
    {
        try
        {
            // Simulate temperature data since real temperature requires WMI access
            var rnd = new Random();
            var cpu = rnd.Next(35, 75);
            var gpu = rnd.Next(40, 85);
            var system = rnd.Next(25, 50);

            CpuTemp = $"{cpu}°C";
            GpuTemp = $"{gpu}°C";
            SystemTemp = $"{system}°C";

            // Determine status based on highest temperature
            var maxTemp = Math.Max(cpu, Math.Max(gpu, system));
            Status = maxTemp switch
            {
                < 50 => "Normal",
                < 70 => "Sıcak",
                < 85 => "Çok Sıcak",
                _ => "Kritik"
            };
        }
        catch
        {
            // Fallback to simulated data
            var rnd = new Random();
            CpuTemp = $"{rnd.Next(40, 70)}°C";
            GpuTemp = $"{rnd.Next(45, 80)}°C";
            SystemTemp = $"{rnd.Next(30, 50)}°C";
            Status = "Normal";
        }
    }

    private void StartTimer()
    {
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(4)
        };
        timer.Tick += (_, _) => LoadTemperatureInfo();
        timer.Start();
    }
}
