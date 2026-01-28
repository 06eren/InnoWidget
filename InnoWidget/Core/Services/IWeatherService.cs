using System.Threading;
using System.Threading.Tasks;

namespace InnoWidget.Core.Services;

public interface IWeatherService
{
    Task<WeatherSnapshot?> GetCurrentAsync(double latitude, double longitude, CancellationToken ct);
}
