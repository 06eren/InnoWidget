using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services;

namespace InnoWidget.Widgets.World;

public sealed class WorldClockWidgetViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly IWeatherService _weather;
    private readonly CancellationTokenSource _cts = new();
    private DateTimeOffset _lastWeatherUpdate = DateTimeOffset.MinValue;
    private bool _isWeatherRefreshing;

    public string Title { get; } = "Dünya Saatleri ve Hava Durumu";

    public ObservableCollection<CityClockItemViewModel> Cities { get; } = new();

    public WorldClockWidgetViewModel(IWeatherService weather)
    {
        _weather = weather;

        Cities.Add(new CityClockItemViewModel("İstanbul", TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"), 41.0082, 28.9784));
        Cities.Add(new CityClockItemViewModel("London", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"), 51.5072, -0.1276));
        Cities.Add(new CityClockItemViewModel("New York", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), 40.7128, -74.0060));

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (_, _) => Refresh();
        _timer.Start();

        _ = RefreshWeatherAsync(force: true);
    }

    private void Refresh()
    {
        foreach (var c in Cities)
            c.UpdateTime();

        _ = RefreshWeatherAsync(force: false);
    }

    private async Task RefreshWeatherAsync(bool force)
    {
        if (_cts.IsCancellationRequested)
            return;

        if (_isWeatherRefreshing)
            return;

        var now = DateTimeOffset.UtcNow;
        if (!force && (now - _lastWeatherUpdate) < TimeSpan.FromMinutes(10))
            return;

        _lastWeatherUpdate = now;

        _isWeatherRefreshing = true;

        try
        {
            foreach (var c in Cities)
            {
                var snap = await _weather.GetCurrentAsync(c.Latitude, c.Longitude, _cts.Token).ConfigureAwait(true);
                if (snap is not null)
                    c.UpdateWeather(snap.Summary, snap.TemperatureC);
            }
        }
        catch
        {
        }
        finally
        {
            _isWeatherRefreshing = false;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _timer.Stop();
    }
}
