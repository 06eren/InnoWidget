using System.ComponentModel;
using InnoWidget.Core.Mvvm;

namespace InnoWidget.Widgets.Weather;

public class WeatherWidgetViewModel : ObservableObject
{
    private string _temperature = "20°C";
    private string _description = "Açık";
    private string _humidity = "65%";
    private string _windSpeed = "10 km/s";
    private string _location = "İstanbul";

    public WeatherWidgetViewModel()
    {
        LoadWeatherData();
        StartTimer();
    }

    public string Temperature
    {
        get => _temperature;
        set => SetProperty(ref _temperature, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Humidity
    {
        get => _humidity;
        set => SetProperty(ref _humidity, value);
    }

    public string WindSpeed
    {
        get => _windSpeed;
        set => SetProperty(ref _windSpeed, value);
    }

    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    private void LoadWeatherData()
    {
        var rnd = new Random();
        var temps = new[] { "18°C", "20°C", "22°C", "24°C", "26°C", "28°C" };
        var descriptions = new[] { "Açık", "Parçalı Bulutlu", "Güneşli", "Az Bulutlu" };
        var humidities = new[] { "45%", "55%", "65%", "75%", "85%" };
        var winds = new[] { "5 km/s", "10 km/s", "15 km/s", "20 km/s" };

        Temperature = temps[rnd.Next(temps.Length)];
        Description = descriptions[rnd.Next(descriptions.Length)];
        Humidity = humidities[rnd.Next(humidities.Length)];
        WindSpeed = winds[rnd.Next(winds.Length)];
    }

    private void StartTimer()
    {
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(10)
        };
        timer.Tick += (_, _) => LoadWeatherData();
        timer.Start();
    }
}
