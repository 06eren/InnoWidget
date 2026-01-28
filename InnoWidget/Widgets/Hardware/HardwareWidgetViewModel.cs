using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services;

namespace InnoWidget.Widgets.Hardware;

public sealed class HardwareWidgetViewModel : ObservableObject, IDisposable
{
    private readonly IMonitoringService<CpuRamSnapshot> _service;
    private readonly DispatcherTimer _timer;
    private bool _isRefreshing;

    private double _cpuPercent;
    public double CpuPercent
    {
        get => _cpuPercent;
        private set => SetProperty(ref _cpuPercent, value);
    }

    private double _ramPercent;
    public double RamPercent
    {
        get => _ramPercent;
        private set => SetProperty(ref _ramPercent, value);
    }

    public string Title { get; } = "Mini Donanım Monitörü";

    public HardwareWidgetViewModel(IMonitoringService<CpuRamSnapshot> service)
    {
        _service = service;

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += async (_, _) => await RefreshAsync().ConfigureAwait(true);

        _ = RefreshAsync();
        _timer.Start();
    }

    private async Task RefreshAsync()
    {
        if (_isRefreshing)
            return;

        _isRefreshing = true;
        try
        {
            var snap = await Task.Run(() => _service.GetSnapshot()).ConfigureAwait(true);
            CpuPercent = snap.CpuPercent;
            RamPercent = snap.RamPercent;
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
