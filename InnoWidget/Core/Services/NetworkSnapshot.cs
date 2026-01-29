namespace InnoWidget.Core.Services;

public sealed record NetworkSnapshot(double DownloadBitsPerSecond, double UploadBitsPerSecond)
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
