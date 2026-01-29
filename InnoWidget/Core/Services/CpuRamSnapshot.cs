namespace InnoWidget.Core.Services;

public sealed record CpuRamSnapshot(double CpuPercent, double RamPercent)
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
