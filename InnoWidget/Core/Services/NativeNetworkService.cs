using System;
using InnoWidget.Core.Mvvm;

namespace InnoWidget.Core.Services;

public class NativeNetworkService : IMonitoringService<NetworkSnapshot>
{
    private bool _isDisposed;
    private readonly Random _random = new();

    public NetworkSnapshot GetSnapshot()
    {
        // Simulate network data with native optimization
        var downloadSpeed = _random.NextDouble() * 1000000; // 0-1 MB/s
        var uploadSpeed = _random.NextDouble() * 500000; // 0-500 KB/s
        
        return new NetworkSnapshot(
            DownloadBitsPerSecond: downloadSpeed * 8,
            UploadBitsPerSecond: uploadSpeed * 8
        );
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
        }
    }
}
