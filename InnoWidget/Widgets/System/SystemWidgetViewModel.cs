using System.ComponentModel;
using InnoWidget.Core.Mvvm;
using Microsoft.Win32;

namespace InnoWidget.Widgets.SystemInfo;

public class SystemWidgetViewModel : ObservableObject
{
    private string _osName = string.Empty;
    private string _osVersion = string.Empty;
    private string _computerName = string.Empty;
    private string _userName = string.Empty;
    private string _uptime = string.Empty;
    private DateTime _startTime = DateTime.Now;

    public SystemWidgetViewModel()
    {
        LoadSystemInfo();
        StartTimer();
    }

    public string OSName
    {
        get => _osName;
        set => SetProperty(ref _osName, value);
    }

    public string OSVersion
    {
        get => _osVersion;
        set => SetProperty(ref _osVersion, value);
    }

    public string ComputerName
    {
        get => _computerName;
        set => SetProperty(ref _computerName, value);
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string Uptime
    {
        get => _uptime;
        set => SetProperty(ref _uptime, value);
    }

    private void LoadSystemInfo()
    {
        try
        {
            OSName = Environment.OSVersion.Platform.ToString();
            OSVersion = Environment.OSVersion.VersionString;
            ComputerName = Environment.MachineName;
            UserName = Environment.UserName;
            _startTime = DateTime.Now.AddMilliseconds(-Environment.TickCount);
            UpdateUptime();
        }
        catch
        {
            OSName = "Bilinmiyor";
            OSVersion = "Bilinmiyor";
            ComputerName = "Bilinmiyor";
            UserName = "Bilinmiyor";
        }
    }

    private void StartTimer()
    {
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += (_, _) => UpdateUptime();
        timer.Start();
    }

    private void UpdateUptime()
    {
        try
        {
            var uptime = DateTime.Now - _startTime;
            Uptime = $"{uptime.Days}g {uptime.Hours}s {uptime.Minutes}d {uptime.Seconds}s";
        }
        catch
        {
            Uptime = "Hesaplanamadı";
        }
    }
}
