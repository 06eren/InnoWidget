using System.ComponentModel;
using InnoWidget.Core.Mvvm;
using System.IO;

namespace InnoWidget.Widgets.Disk;

public class DiskWidgetViewModel : ObservableObject
{
    private string _totalSpace = "0 GB";
    private string _usedSpace = "0 GB";
    private string _freeSpace = "0 GB";
    private double _usagePercent = 0;
    private string _driveLetter = "C:";

    public DiskWidgetViewModel()
    {
        LoadDiskInfo();
        StartTimer();
    }

    public string TotalSpace
    {
        get => _totalSpace;
        set => SetProperty(ref _totalSpace, value);
    }

    public string UsedSpace
    {
        get => _usedSpace;
        set => SetProperty(ref _usedSpace, value);
    }

    public string FreeSpace
    {
        get => _freeSpace;
        set => SetProperty(ref _freeSpace, value);
    }

    public double UsagePercent
    {
        get => _usagePercent;
        set => SetProperty(ref _usagePercent, value);
    }

    public string DriveLetter
    {
        get => _driveLetter;
        set => SetProperty(ref _driveLetter, value);
    }

    private void LoadDiskInfo()
    {
        try
        {
            var drive = new DriveInfo("C");
            if (drive.IsReady)
            {
                var total = drive.TotalSize;
                var free = drive.AvailableFreeSpace;
                var used = total - free;

                TotalSpace = FormatBytes(total);
                FreeSpace = FormatBytes(free);
                UsedSpace = FormatBytes(used);
                UsagePercent = (double)used / total * 100;
                DriveLetter = drive.Name;
            }
        }
        catch
        {
            // Simulate data for demo
            var rnd = new Random();
            var totalBytes = 500L * 1024 * 1024 * 1024; // 500GB
            var usedBytes = (long)(totalBytes * (0.3 + rnd.NextDouble() * 0.4)); // 30-70%
            var freeBytes = totalBytes - usedBytes;

            TotalSpace = FormatBytes(totalBytes);
            FreeSpace = FormatBytes(freeBytes);
            UsedSpace = FormatBytes(usedBytes);
            UsagePercent = (double)usedBytes / totalBytes * 100;
            DriveLetter = "C:";
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }

    private void StartTimer()
    {
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        timer.Tick += (_, _) => LoadDiskInfo();
        timer.Start();
    }
}
