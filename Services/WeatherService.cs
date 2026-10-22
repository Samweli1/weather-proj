using System.Net.Http.Json;
using weather_proj.Models;

namespace weather_proj.Services;


public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly string _apiKey;
    private const string BaseUrl = "http://dataservice.accuweather.com";

    public WeatherService(HttpClient httpClient, ISettingsService settingsService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        _apiKey = Environment.GetEnvironmentVariable("ACCUWEATHER_API_KEY") ?? "YOUR_API_KEY_HERE";
    }

    private async Task<string?> GetLocationKeyAsync(string cityName)
    {
        string url = $"{BaseUrl}/locations/v1/cities/search?apikey={_apiKey}&q={Uri.EscapeDataString(cityName)}";
        var locations = await _httpClient.GetFromJsonAsync<List<AccuLocationDto>>(url);

        return locations?.FirstOrDefault()?.Key;
    }
    
    private async Task<List<ForecastItem>> FetchForecastAsync()
    {
        string city = _settingsService.GetCurrentCity();
        string? locationKey = await GetLocationKeyAsync(city);

        if (string.IsNullOrEmpty(locationKey))
        {
            return new List<ForecastItem>();
        }
        string url = $"{BaseUrl}/forecasts/v1/daily/5day/{locationKey}?apikey={_apiKey}&metric=true";
        var response = await _httpClient.GetFromJsonAsync<AccuWeatherResponse>(url);

        if (response?.DailyForecasts == null)
        {
            return new List<ForecastItem>();
        }

        var result = new List<ForecastItem>();
        foreach (var day in response.DailyForecasts)
        {
            result.Add(new ForecastItem
            {
                Date = day.Date,
                MinTemp = day.Temperature.Minimum.Value,
                MaxTemp = day.Temperature.Maximum.Value,
                Description = day.Day.IconPhrase
                    
            });
        }

        return result;
    }
    
    public async Task<List<ForecastItem>> GetForecastWeekAsync()
    {
        var forecast = await FetchForecastAsync();

        _settingsService.AddEntryToHistory(new HistoryRecord
        {
            RequestedAt = DateTime.Now,
            City = _settingsService.GetCurrentCity(),
            PeriodDescription = "Прогноз на неделю"
        });

        return forecast;
    }

    public async Task<ForecastItem?> GetForecastDayAsync()
    {
        var forecast = await FetchForecastAsync();
        var tomorrow = DateTime.Today.AddDays(1);

        var item = forecast.FirstOrDefault(x => x.Date.Date == tomorrow.Date);

        _settingsService.AddEntryToHistory(new HistoryRecord
        {
            RequestedAt = DateTime.Now,
            City = _settingsService.GetCurrentCity(),
            PeriodDescription = "Прогноз на следующий день"
        });

        return item;
    }

    public async Task<ForecastItem?> GetForecastSpecificDateAsync(DateTime date)
    {
        var forecast = await FetchForecastAsync();

        var item = forecast.FirstOrDefault(x => x.Date.Date == date.Date);

        _settingsService.AddEntryToHistory(new HistoryRecord
        {
            RequestedAt = DateTime.Now,
            City = _settingsService.GetCurrentCity(),
            PeriodDescription = $"Прогноз на {date:dd.MM.yyyy}"
        });

        return item;
    }

    public async Task<List<ForecastItem>> GetForecastForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var forecast = await FetchForecastAsync();

        var items = forecast
            .Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date)
            .ToList();

        _settingsService.AddEntryToHistory(new HistoryRecord
        {
            RequestedAt = DateTime.Now,
            City = _settingsService.GetCurrentCity(),
            PeriodDescription = $"Период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}"
        });

        return items;
    }
}


