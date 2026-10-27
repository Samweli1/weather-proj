namespace weather_proj.Models;

public class AccuTemperature
{
    public AccuTempValue Minimum { get; set; } = new();
    public AccuTempValue Maximum { get; set; } = new();
}

public class AccuTempValue
{
    public double Value { get; set; }
}

public class AccuDayInfo
{
    public string IconPhrase { get; set; } = string.Empty;
}

public class AccuLocationDto
{
    public string Key { get; set; } = string.Empty;
}

public class AccuWeatherResponse
{
    public List<AccuDailyForecast> DailyForecasts { get; set; } = new();
}

public class AccuDailyForecast
{
    public DateTime Date { get; set; }
    public AccuTemperature Temperature { get; set; } = new();
    public AccuDayInfo Day { get; set; } = new();
}