using System.Diagnostics;
using System.Runtime.InteropServices;

namespace InnoWidget.Core.Services;

public class HighPerformanceTimer : IDisposable
{
    private static readonly Lazy<HighPerformanceTimer> _instance = new(() => new HighPerformanceTimer());
    public static HighPerformanceTimer Instance => _instance.Value;

    private readonly Stopwatch _stopwatch = new();
    private long _frequency;
    private bool _isHighResolution;

    [DllImport("kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(out long lpFrequency);

    [DllImport("kernel32.dll")]
    private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

    private HighPerformanceTimer()
    {
        _isHighResolution = QueryPerformanceFrequency(out _frequency);
        if (_isHighResolution)
        {
            QueryPerformanceCounter(out long _);
        }
        else
        {
            _frequency = Stopwatch.Frequency;
        }
    }

    public void Start()
    {
        if (_isHighResolution)
        {
            QueryPerformanceCounter(out long _);
        }
        else
        {
            _stopwatch.Restart();
        }
    }

    public double GetElapsedMilliseconds()
    {
        if (_isHighResolution)
        {
            QueryPerformanceCounter(out long end);
            return (double)(end * 1000) / _frequency;
        }
        else
        {
            return _stopwatch.Elapsed.TotalMilliseconds;
        }
    }

    public void Dispose()
    {
        _stopwatch?.Stop();
    }
}
