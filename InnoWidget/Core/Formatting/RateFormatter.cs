using System;

namespace InnoWidget.Core.Formatting;

public static class RateFormatter
{
    public static string BitsPerSecondToString(double bitsPerSecond)
    {
        if (double.IsNaN(bitsPerSecond) || double.IsInfinity(bitsPerSecond) || bitsPerSecond < 0)
            bitsPerSecond = 0;

        var units = new[] { "bps", "Kbps", "Mbps", "Gbps" };
        var value = bitsPerSecond;
        var unitIndex = 0;

        while (value >= 1000 && unitIndex < units.Length - 1)
        {
            value /= 1000;
            unitIndex++;
        }

        return $"{value:0.##} {units[unitIndex]}";
    }
}
