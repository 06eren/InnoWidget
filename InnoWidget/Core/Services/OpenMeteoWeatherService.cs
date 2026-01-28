using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace InnoWidget.Core.Services;

public sealed class OpenMeteoWeatherService : IWeatherService
{
    private readonly HttpClient _http;

    public OpenMeteoWeatherService(HttpClient http)
    {
        _http = http;
    }

    public async Task<WeatherSnapshot?> GetCurrentAsync(double latitude, double longitude, CancellationToken ct)
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude:0.####}&longitude={longitude:0.####}&current=temperature_2m,weather_code";
        using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);

        if (!doc.RootElement.TryGetProperty("current", out var current))
            return null;

        var temp = current.TryGetProperty("temperature_2m", out var t) ? t.GetDouble() : double.NaN;
        var code = current.TryGetProperty("weather_code", out var c) ? c.GetInt32() : -1;

        var summary = WeatherCodeToSummary(code);
        return new WeatherSnapshot(summary, temp);
    }

    private static string WeatherCodeToSummary(int code)
    {
        return code switch
        {
            0 => "Clear",
            1 or 2 or 3 => "Partly Cloudy",
            45 or 48 => "Fog",
            51 or 53 or 55 => "Drizzle",
            61 or 63 or 65 => "Rain",
            71 or 73 or 75 => "Snow",
            80 or 81 or 82 => "Showers",
            95 or 96 or 99 => "Thunder",
            _ => "—"
        };
    }
}
