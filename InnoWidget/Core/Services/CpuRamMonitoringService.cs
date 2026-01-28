using System;
using System.Diagnostics;

namespace InnoWidget.Core.Services;

public sealed class CpuRamMonitoringService : IMonitoringService<CpuRamSnapshot>, IDisposable
{
    private readonly PerformanceCounter _cpu;
    private readonly PerformanceCounter _ram;

    public CpuRamMonitoringService()
    {
        _cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _ram = new PerformanceCounter("Memory", "% Committed Bytes In Use");

        _ = _cpu.NextValue();
        _ = _ram.NextValue();
    }

    public CpuRamSnapshot GetSnapshot()
    {
        var cpu = _cpu.NextValue();
        var ram = _ram.NextValue();

        cpu = Math.Clamp(cpu, 0, 100);
        ram = Math.Clamp(ram, 0, 100);

        return new CpuRamSnapshot(cpu, ram);
    }

    public void Dispose()
    {
        _cpu.Dispose();
        _ram.Dispose();
    }
}
