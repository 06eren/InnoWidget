#include <windows.h>
#include <psapi.h>
#include <pdh.h>
#include <iostream>
#include <memory>

#pragma comment(lib, "psapi.lib")
#pragma comment(lib, "pdh.lib")

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

    __declspec(dllexport) SystemInfo GetSystemInfo() {
        SystemInfo info = {0};
        
        // CPU Usage
        static ULARGE_INTEGER lastCPU, lastSysCPU, lastUserCPU;
        static int numProcessors = 0;
        static HANDLE self = GetCurrentProcess();
        
        if (numProcessors == 0) {
            SYSTEM_INFO sysInfo;
            GetSystemInfo(&sysInfo);
            numProcessors = sysInfo.dwNumberOfProcessors;
            
            FILETIME ftime, fsys, fuser;
            GetSystemTimeAsFileTime(&ftime);
            memcpy(&lastCPU, &ftime, sizeof(FILETIME));
            
            GetProcessTimes(self, &ftime, &ftime, &fsys, &fuser);
            memcpy(&lastSysCPU, &fsys, sizeof(FILETIME));
            memcpy(&lastUserCPU, &fuser, sizeof(FILETIME));
        }
        
        FILETIME ftime, fsys, fuser;
        ULARGE_INTEGER now, sys, user;
        
        GetSystemTimeAsFileTime(&ftime);
        memcpy(&now, &ftime, sizeof(FILETIME));
        
        GetProcessTimes(self, &ftime, &ftime, &fsys, &fuser);
        memcpy(&sys, &fsys, sizeof(FILETIME));
        memcpy(&user, &fuser, sizeof(FILETIME));
        
        double percent = (sys.QuadPart - lastSysCPU.QuadPart) + (user.QuadPart - lastUserCPU.QuadPart);
        percent /= (now.QuadPart - lastCPU.QuadPart);
        percent /= numProcessors;
        info.cpuUsage = percent * 100;
        
        lastCPU = now;
        lastUserCPU = user;
        lastSysCPU = sys;
        
        // RAM Usage
        MEMORYSTATUSEX memInfo;
        memInfo.dwLength = sizeof(MEMORYSTATUSEX);
        GlobalMemoryStatusEx(&memInfo);
        
        info.ramTotal = (double)memInfo.ullTotalPhys / (1024 * 1024 * 1024);
        info.ramAvailable = (double)memInfo.ullAvailPhys / (1024 * 1024 * 1024);
        info.ramUsage = ((double)(memInfo.ullTotalPhys - memInfo.ullAvailPhys) / memInfo.ullTotalPhys) * 100;
        
        // Process Count
        DWORD aProcesses[1024], cbNeeded;
        if (EnumProcesses(aProcesses, sizeof(aProcesses), &cbNeeded)) {
            info.processCount = cbNeeded / sizeof(DWORD);
        }
        
        // Disk Usage
        ULARGE_INTEGER freeBytesAvailable, totalBytes, freeBytes;
        if (GetDiskFreeSpaceExEx(L"C:", &freeBytesAvailable, &totalBytes, &freeBytes)) {
            info.diskTotal = (double)totalBytes.QuadPart / (1024 * 1024 * 1024);
            info.diskFree = (double)freeBytes.QuadPart / (1024 * 1024 * 1024);
            info.diskUsage = ((double)(totalBytes.QuadPart - freeBytes.QuadPart) / totalBytes.QuadPart) * 100;
        }
        
        // Simulate temperatures (real temperature requires WMI or specific hardware APIs)
        info.cpuTemp = 45.0 + (rand() % 30);
        info.gpuTemp = 50.0 + (rand() % 35);
        info.systemTemp = 35.0 + (rand() % 20);
        
        return info;
    }
    
    __declspec(dllexport) void OptimizePerformance() {
        // Set process priority to high
        SetPriorityClass(GetCurrentProcess(), HIGH_PRIORITY_CLASS);
        
        // Disable Windows error reporting for this process
        SetErrorMode(SEM_NOGPFAULTERRORBOX);
        
        // Set thread affinity to avoid context switches
        HANDLE hThread = GetCurrentThread();
        SetThreadAffinityMask(hThread, 0x1);
        
        // Flush file buffers to reduce I/O overhead
        FlushFileBuffers(GetStdHandle(STD_OUTPUT_HANDLE));
        
        // Optimize timer resolution
        timeBeginPeriod(1);
    }
    
    __declspec(dllexport) void RestorePerformance() {
        // Restore normal priority
        SetPriorityClass(GetCurrentProcess(), NORMAL_PRIORITY_CLASS);
        
        // Restore timer resolution
        timeEndPeriod(1);
    }
}
