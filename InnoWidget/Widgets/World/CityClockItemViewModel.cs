using System;
using InnoWidget.Core.Mvvm;

namespace InnoWidget.Widgets.World;

public sealed class CityClockItemViewModel : ObservableObject
{
    public string City { get; }
    public TimeZoneInfo TimeZone { get; }
    public double Latitude { get; }
    public double Longitude { get; }

    private string _timeText = "";
    public string TimeText
    {
        get => _timeText;
        private set => SetProperty(ref _timeText, value);
    }

    private string _weatherText = "—";
    public string WeatherText
    {
        get => _weatherText;
        private set => SetProperty(ref _weatherText, value);
    }

    public CityClockItemViewModel(string city, TimeZoneInfo timeZone, double latitude, double longitude)
    {
        City = city;
        TimeZone = timeZone;
        Latitude = latitude;
        Longitude = longitude;
        UpdateTime();
    }

    public void UpdateTime()
    {
        var local = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZone);
        TimeText = local.ToString("HH:mm");
    }

    public void UpdateWeather(string summary, double temperatureC)
    {
        if (double.IsNaN(temperatureC) || double.IsInfinity(temperatureC))
            WeatherText = summary;
        else
            WeatherText = $"{summary} • {temperatureC:0.#}°C";
    }
}
