namespace InnoWidget.Core.Services;

public interface IMonitoringService<out T>
{
    T GetSnapshot();
}
