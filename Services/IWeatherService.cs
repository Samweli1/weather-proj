using weather_proj.Models;

namespace weather_proj.Services;

public interface IWeatherService
{
    public Task<List<ForecastItem>> GetForecastWeekAsync();
    public Task<ForecastItem?> GetForecastDayAsync();
    public Task<ForecastItem?> GetForecastSpecificDateAsync(DateTime date);
    public Task<List<ForecastItem>> GetForecastForPeriodAsync(DateTime startDate, DateTime endDate);
}