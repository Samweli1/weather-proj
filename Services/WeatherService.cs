using System.Net.Http.Json;
using weather_proj.Configuration;
using weather_proj.Models;
using Microsoft.Extensions.Options;
using weather_proj.Common;

namespace weather_proj.Services;


public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly AccuWeatherOptions _options;

    public WeatherService(
        HttpClient httpClient,
        IOptions<AccuWeatherOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    private async Task<Result<string?>> GetLocationKeyAsync(string cityName)
    {
        string url = $"{_options.BaseUrl}/locations/v1/cities/search?apikey={_options.ApiKey}&q={Uri.EscapeDataString(cityName)}";

        try
        {
            var locations = await _httpClient.GetFromJsonAsync<List<AccuLocationDto>>(url);
            var key =  locations?.FirstOrDefault()?.Key;

            if (string.IsNullOrWhiteSpace(key))
            {
                return Result<string?>.Failure($"Город '{cityName}' не найден в базе AccuWeather.");
            }
            return  Result<string?>.Success(key);
        }
        catch (HttpRequestException ex)
        {
            return Result<string?>.Failure($"Network error while searching for a city: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<string?>.Failure($"The request timed out while searching for the city.");
        }

    }

    private async Task<Result<List<ForecastItem>>> FetchForecastAsync(string city)
    {
        var locationResult = await GetLocationKeyAsync(city);

        if (locationResult.IsFailure)
        {
            return Result<List<ForecastItem>>.Failure(locationResult.Error!);
        }

        string locationKey = locationResult.Value!;
        string url = $"{_options.BaseUrl}/forecasts/v1/daily/5day/{locationKey}?apikey={_options.ApiKey}&metric=true";
        
        try
        {
            var response = await _httpClient.GetFromJsonAsync<AccuWeatherResponse>(url);

            if (response?.DailyForecasts == null || response.DailyForecasts.Count == 0)
            {
                return Result<List<ForecastItem>>.Failure($"No forecast data available for the city '{city}'.");
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

            return Result<List<ForecastItem>>.Success(result);
        }
        
        catch (HttpRequestException ex)
        {
           return Result<List<ForecastItem>>.Failure($"Network error while retrieving the forecast: {ex.Message}");
        }
    }

    public Task<Result<List<ForecastItem>>> GetForecastWeekAsync(string city) => 
        FetchForecastAsync(city);

    public async Task<Result<ForecastItem>> GetForecastDayAsync(string city)
    {
        var forecastResult = await FetchForecastAsync(city);

        if (forecastResult.IsFailure)
        {
            return Result<ForecastItem>.Failure(forecastResult.Error!);
        }

        var tomorrow = DateTime.Today.AddDays(1);
        var item = forecastResult.Value!.FirstOrDefault(x => x.Date.Date == tomorrow.Date);

        if (item is null)
        {
            return Result<ForecastItem>.Failure("The forecast for tomorrow is missing from the data received.");
        }

        return Result<ForecastItem>.Success(item);
    }

    public async Task<Result<ForecastItem>> GetForecastSpecificDateAsync(string city, DateTime date)
    {
        var forecastResult = await FetchForecastAsync(city);

        if (forecastResult.IsFailure)
        {
            return Result<ForecastItem>.Failure(forecastResult.Error!);
        }

        var item = forecastResult.Value!.FirstOrDefault(x => x.Date.Date == date.Date);

        if (item is null)
        {
          return Result<ForecastItem>.Failure($"Forecast for date {date:dd.MM.yyyy} not found");
        }

        return Result<ForecastItem>.Success(item);
    }

    public async Task<Result<List<ForecastItem>>> GetForecastForPeriodAsync(string city, DateTime startDate, DateTime endDate)
    {
        var forecastResult = await FetchForecastAsync(city);

        if (forecastResult.IsFailure)
        {
            return Result<List<ForecastItem>>.Failure(forecastResult.Error!);
        }

        var items = forecastResult.Value!
            .Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date)
            .ToList();

        if (items.Count == 0)
        {
           return  Result<List<ForecastItem>>.Failure("No forecast data available for the period from {startDate:dd.MM.yyyy} to {endDate:dd.MM.yyyy}.");
        }
        return Result<List<ForecastItem>>.Success(items);
    }
}


