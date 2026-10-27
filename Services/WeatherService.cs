using System.Net.Http.Json;
using System.Text.Json;
using weather_proj.Configuration;
using weather_proj.Models;

namespace weather_proj.Services;


public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly AccuWeatherOptions _options;

    public WeatherService(HttpClient httpClient, ISettingsService settingsService,  AccuWeatherOptions options)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        _options = options;
    }

    private async Task<string?> GetLocationKeyAsync(string cityName)
    {
        string url = $"{_options.BaseUrl}/locations/v1/cities/search?apikey={_options.ApiKey}&q={Uri.EscapeDataString(cityName)}";

        try
        {
            var locations = await _httpClient.GetFromJsonAsync<List<AccuLocationDto>>(url);
            return locations?.FirstOrDefault()?.Key;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(
                $"[Network error] Unable to connect to the weather service: {ex.StatusCode} - {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[Parse error] server returned an invalid response: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"[TimeOut error] The time for waiting for a response from the server has expired.");
            return null;
        }
      
    }
    
    private async Task<List<ForecastItem>> FetchForecastAsync()
    {
        string city = _settingsService.CurrentCity;
        string? locationKey = await GetLocationKeyAsync(city);

        if (string.IsNullOrWhiteSpace(locationKey)) 
        {
            Console.WriteLine($"[Warning] Could not determine the location key for the city: {city}");
            return new List<ForecastItem>();
        }
        
        string url = $"{_options.BaseUrl}/forecasts/v1/daily/5day/{locationKey}?apikey={_options.ApiKey}&metric=true";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<AccuWeatherResponse>(url);

            if (response?.DailyForecasts == null || response.DailyForecasts.Count == 0)
            {
                Console.WriteLine(
                    $"[Info] There is no weather forecast available for the city of '{city}' for the coming days.");
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
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized
                                              || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            Console.WriteLine($"[Authorization error] Invalid API key or request limit exceeded.");
            return new List<ForecastItem>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[Network error] Unable to connect to the weather service: {ex.Message}");
            return new List<ForecastItem>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[Data error] The weather service's response is in an unexpected format: {ex.Message}");
            return new List<ForecastItem>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Network error] Unexpected error: {ex.Message}");
            return new List<ForecastItem>();   
        }
    
    }
    
    public async Task<List<ForecastItem>> GetForecastWeekAsync()
    {
        
        var forecast = await FetchForecastAsync();

        if (forecast.Count == 0)
        {
            return forecast;
        }

        _settingsService.AddEntryToHistory(new HistoryRecord
        {
            RequestedAt = DateTime.Now,
            City = _settingsService.CurrentCity,
            PeriodDescription = "Forecast for the week"
        });

        return forecast;
    }

    public async Task<ForecastItem?> GetForecastDayAsync()
    {
        var forecast = await FetchForecastAsync();
        
        if (forecast.Count == 0)
        {
            return null;
        }
        
        var tomorrow = DateTime.Today.AddDays(1);
        var item = forecast.FirstOrDefault(x => x.Date.Date == tomorrow.Date);

        if (item != null)
        {
            _settingsService.AddEntryToHistory(new HistoryRecord
            {
                RequestedAt = DateTime.Now,
                City = _settingsService.CurrentCity,
                PeriodDescription = "Forecast for the next day"
            });
        }
        
        return item;
    }

    public async Task<ForecastItem?> GetForecastSpecificDateAsync(DateTime date)
    {
        var forecast = await FetchForecastAsync();

        if (forecast.Count == 0)
        {
            return null;
        }

        var item = forecast.FirstOrDefault(x => x.Date.Date == date.Date);

        if (item != null)
        {
            _settingsService.AddEntryToHistory(new HistoryRecord
            {
                RequestedAt = DateTime.Now,
                City = _settingsService.CurrentCity,
                PeriodDescription = $"Forecast on {date:dd.MM.yyyy}"
            });
        }
        
        return item;
    }

    public async Task<List<ForecastItem>> GetForecastForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var forecast = await FetchForecastAsync();
        
        if (forecast.Count == 0)
        {
            return forecast;
        }

        var items = forecast
            .Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date)
            .ToList();

        if (items.Count > 0)
        {
                  
            _settingsService.AddEntryToHistory(new HistoryRecord
            {
                RequestedAt = DateTime.Now,
                City = _settingsService.CurrentCity,
                PeriodDescription = $"Period from {startDate:dd.MM.yyyy} to {endDate:dd.MM.yyyy}"
            });
        }
        return items;
    }
}


