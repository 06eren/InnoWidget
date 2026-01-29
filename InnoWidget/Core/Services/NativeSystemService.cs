using System.Runtime.InteropServices;

namespace InnoWidget.Core.Services;

public class NativeSystemService
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemInfo
    {
        public double CpuUsage;
        public double RamUsage;
        public double RamTotal;
        public double RamAvailable;
        public int ProcessCount;
        public double DiskUsage;
        public double DiskTotal;
        public double DiskFree;
        public double CpuTemp;
        public double GpuTemp;
        public double SystemTemp;
    }

    [DllImport("NativePerformance.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern SystemInfo GetSystemInfo();

    [DllImport("NativePerformance.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void OptimizePerformance();

    [DllImport("NativePerformance.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void RestorePerformance();

    private static bool _isOptimized = false;

    public static SystemInfo GetOptimizedSystemInfo()
    {
        if (!_isOptimized)
        {
            OptimizePerformance();
            _isOptimized = true;
        }
        
        return GetSystemInfo();
    }

    public static void Cleanup()
    {
        if (_isOptimized)
        {
            RestorePerformance();
            _isOptimized = false;
        }
    }
}
