using System;
using InnoWidget.Core.Mvvm;

namespace InnoWidget.Core.Services;

public class NativeHardwareService : IMonitoringService<CpuRamSnapshot>
{
    private bool _isDisposed;

    public CpuRamSnapshot GetSnapshot()
    {
        var nativeInfo = NativeSystemService.GetOptimizedSystemInfo();
        
        return new CpuRamSnapshot(
            CpuPercent: nativeInfo.CpuUsage,
            RamPercent: nativeInfo.RamUsage
        );
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            NativeSystemService.Cleanup();
            _isDisposed = true;
        }
    }
}
