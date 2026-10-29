using weather_proj.Common;
using weather_proj.Models;

namespace weather_proj.Services;

public interface IWeatherService
{
    public Task<Result<List<ForecastItem>>> GetForecastWeekAsync(string city);
    public Task<Result<ForecastItem>> GetForecastDayAsync(string city);
    public Task<Result<ForecastItem>> GetForecastSpecificDateAsync(string city, DateTime date);
    public Task<Result<List<ForecastItem>>> GetForecastForPeriodAsync(string city, DateTime startDate, DateTime endDate);
}