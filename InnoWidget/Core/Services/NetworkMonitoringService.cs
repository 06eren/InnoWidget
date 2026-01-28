using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace InnoWidget.Core.Services;

public sealed class NetworkMonitoringService : IMonitoringService<NetworkSnapshot>
{
    private long _lastBytesReceived;
    private long _lastBytesSent;
    private DateTimeOffset _lastSampleAt;

    public NetworkMonitoringService()
    {
        var (rx, tx) = ReadTotals();
        _lastBytesReceived = rx;
        _lastBytesSent = tx;
        _lastSampleAt = DateTimeOffset.UtcNow;
    }

    public NetworkSnapshot GetSnapshot()
    {
        var now = DateTimeOffset.UtcNow;
        var (rx, tx) = ReadTotals();

        var elapsed = (now - _lastSampleAt).TotalSeconds;
        if (elapsed <= 0)
            elapsed = 1;

        var downloadBytesPerSecond = (rx - _lastBytesReceived) / elapsed;
        var uploadBytesPerSecond = (tx - _lastBytesSent) / elapsed;

        _lastBytesReceived = rx;
        _lastBytesSent = tx;
        _lastSampleAt = now;

        var downloadBitsPerSecond = Math.Max(0, downloadBytesPerSecond) * 8.0;
        var uploadBitsPerSecond = Math.Max(0, uploadBytesPerSecond) * 8.0;

        return new NetworkSnapshot(downloadBitsPerSecond, uploadBitsPerSecond);
    }

    private static (long bytesReceived, long bytesSent) ReadTotals()
    {
        long rx = 0;
        long tx = 0;

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            var stats = ni.GetIPv4Statistics();
            rx += stats.BytesReceived;
            tx += stats.BytesSent;
        }

        return (rx, tx);
    }
}
