using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace InnoWidget.Core.Services;

public class PerformanceMonitor : IDisposable
{
    private DispatcherTimer? _timer;
    private PerformanceCounter? _cpuCounter;
    private PerformanceCounter? _memoryCounter;
    private bool _disposed;

    public double CpuUsage { get; private set; }
    public double MemoryUsage { get; private set; }

    public PerformanceMonitor()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            
            // İlk okuma için bekle
            Task.Delay(1000).Wait();
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) // 5 saniyede bir kontrol
            };
            _timer.Tick += (_, _) => UpdateMetrics();
            _timer.Start();
        }
        catch
        {
            // Performance counter'lar çalışmazsa sessizce geç
        }
    }

    private void UpdateMetrics()
    {
        if (_disposed) return;

        try
        {
            if (_cpuCounter != null)
                CpuUsage = _cpuCounter.NextValue();
                
            if (_memoryCounter != null)
            {
                var availableMB = _memoryCounter.NextValue();
                var totalMB = GC.GetTotalMemory(false) / (1024 * 1024);
                MemoryUsage = Math.Max(0, totalMB - availableMB);
            }
        }
        catch
        {
            // Hata durumunda sessizce geç
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _timer?.Stop();
        
        try
        {
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
        }
        catch
        {
            // Dispose sırasında hata olursa sessizce geç
        }
    }
}
