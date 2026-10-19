using Weatherproj.Models;

namespace Weatherproj.Services;

public interface IWeatherService
{
    Task<List<ForecastItem>> GetForecastWeekAsync();
    Task<ForecastItem?> GetForecastDayAsync();
    Task<ForecastItem?> GetForecastSpecificDateAsync(DateTime date);
    Task<List<ForecastItem>> GetForecastForPeriodAsync(DateTime startDate, DateTime endDate);
}