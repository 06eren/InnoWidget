using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System;
using System.Runtime;

namespace InnoWidget.Core.Services;

public class PerformanceOptimizer
{
    private static readonly Lazy<PerformanceOptimizer> _instance = new(() => new PerformanceOptimizer());
    public static PerformanceOptimizer Instance => _instance.Value;

    private bool _isOptimized;
    private readonly DispatcherTimer _monitorTimer;

    private PerformanceOptimizer()
    {
        _monitorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        _monitorTimer.Tick += MonitorPerformance;
        _monitorTimer.Start();
    }

    public void OptimizeApplication()
    {
        if (_isOptimized) return;

        // Set process priority
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

        // Optimize garbage collection
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;

        // Optimize animation engine
        OptimizedAnimationEngine.Instance.OptimizeForPerformance();

        // Native optimizations
        NativeSystemService.OptimizePerformance();

        _isOptimized = true;
    }

    public void RestoreApplication()
    {
        if (!_isOptimized) return;

        // Restore process priority
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;

        // Restore garbage collection
        GCSettings.LatencyMode = GCLatencyMode.Interactive;

        // Restore native settings
        NativeSystemService.RestorePerformance();

        _isOptimized = false;
    }

    private void MonitorPerformance(object? sender, EventArgs e)
    {
        var process = Process.GetCurrentProcess();
        var memoryMB = process.WorkingSet64 / 1024 / 1024;
        var cpuUsage = process.TotalProcessorTime.TotalMilliseconds;

        // Auto-optimize if memory usage is high
        if (memoryMB > 200)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        // Log performance metrics (can be removed in production)
        Debug.WriteLine($"Memory: {memoryMB}MB, CPU Time: {cpuUsage}ms");
    }

    public PerformanceMetrics GetMetrics()
    {
        var process = Process.GetCurrentProcess();
        return new PerformanceMetrics
        {
            MemoryUsageMB = process.WorkingSet64 / 1024 / 1024,
            CpuTimeMs = process.TotalProcessorTime.TotalMilliseconds,
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            IsOptimized = _isOptimized
        };
    }
}

public class PerformanceMetrics
{
    public long MemoryUsageMB { get; set; }
    public double CpuTimeMs { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public bool IsOptimized { get; set; }
}
