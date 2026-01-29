#pragma once

extern "C" {
    struct SystemInfo {
        double cpuUsage;
        double ramUsage;
        double ramTotal;
        double ramAvailable;
        int processCount;
        double diskUsage;
        double diskTotal;
        double diskFree;
        double cpuTemp;
        double gpuTemp;
        double systemTemp;
    };

    __declspec(dllexport) SystemInfo GetSystemInfo();
    __declspec(dllexport) void OptimizePerformance();
    __declspec(dllexport) void RestorePerformance();
}
