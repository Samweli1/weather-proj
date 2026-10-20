using Weatherproj.Models;

namespace Weatherproj.Services;

public interface IWeatherService
{
    public Task<List<ForecastItem>> GetForecastWeekAsync();
    public Task<ForecastItem?> GetForecastDayAsync();
    public Task<ForecastItem?> GetForecastSpecificDateAsync(DateTime date);
    public Task<List<ForecastItem>> GetForecastForPeriodAsync(DateTime startDate, DateTime endDate);
}