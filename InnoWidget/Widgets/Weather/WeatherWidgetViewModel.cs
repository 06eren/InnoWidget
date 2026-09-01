using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services;

namespace InnoWidget.Widgets.Weather;

public class WeatherWidgetViewModel : ObservableObject, IDisposable
{
    private readonly IWeatherService _weatherService;
    private bool _isRefreshing;

    private string _temperature = "20°C";
    private string _description = "Açık";
    private string _humidity = "65%";
    private string _windSpeed = "10 km/s";
    private string _location = "İstanbul";

    public WeatherWidgetViewModel(IWeatherService? weatherService = null)
    {
        _weatherService = weatherService ?? new OpenMeteoWeatherService(new System.Net.Http.HttpClient());
        
        // TIMER KALDIRILDI - KASMA SORUNU KÖKEN ÇÖZÜM
        // Sadece başlangıçta bir kez veri al
        LoadWeatherDataOnce();
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

    private async void LoadWeatherDataOnce()
    {
        if (_isRefreshing || _weatherService == null)
            return;

        _isRefreshing = true;
        try
        {
            // Istanbul koordinatları - sadece bir kez
            var weather = await _weatherService.GetCurrentAsync(41.015137, 28.979530, CancellationToken.None);
            
            if (weather != null)
            {
                Temperature = $"{weather.TemperatureC:F1}°C";
                Description = weather.Summary;
                Humidity = "N/A"; // WeatherSnapshot'ta bu property yok
                WindSpeed = "N/A"; // WeatherSnapshot'ta bu property yok
                Location = "İstanbul";
            }
        }
        catch
        {
            // API hatası durumunda varsayılan değerler
            Temperature = "N/A";
            Description = "Veri alınamadı";
            Humidity = "N/A";
            WindSpeed = "N/A";
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private string GetWeatherDescription(int weatherCode)
    {
        return weatherCode switch
        {
            0 => "Açık",
            1 => "Çok az bulutlu",
            2 => "Az bulutlu",
            3 => "Parçalı bulutlu",
            45 => "Sisli",
            48 => "Sisli",
            51 => "Hafif yağmurlu",
            53 => "Yağmurlu",
            55 => "Yoğun yağmurlu",
            56 => "Hafif dondurucu yağmurlu",
            57 => "Dondurucu yağmurlu",
            61 => "Hafif yağmurlu",
            63 => "Yağmurlu",
            65 => "Yoğun yağmurlu",
            66 => "Hafif dondurucu yağmurlu",
            67 => "Dondurucu yağmurlu",
            71 => "Hafif karlı",
            73 => "Karlı",
            75 => "Yoğun karlı",
            77 => "Kar taneli",
            80 => "Hafif sağanaklı",
            81 => "Sağanaklı",
            82 => "Yoğun sağanaklı",
            85 => "Hafif sağanaklı",
            86 => "Yoğun sağanaklı",
            95 => "Gök gürültülü hafif yağmurlu",
            96 => "Gök gürültülü hafif dolulu yağmurlu",
            99 => "Yoğun gök gürültülü dolulu yağmurlu",
            _ => "Bilinmeyen"
        };
    }

    public void Dispose()
    {
        // TIMER KALDIRILDI - KASMA SORUNU KÖKEN ÇÖZÜM
        // Dispose gerekli değil - timer yok
    }
}
