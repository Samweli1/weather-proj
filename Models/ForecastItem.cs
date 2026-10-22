namespace weather_proj.Models;

public class ForecastItem
{
    public DateTime Date { get; set; }
    public  double MinTemp {  get; set; }
    public  double MaxTemp {  get; set; }
    public string Description { get; set;  } = string.Empty;
}

public class HistoryRecord
{
    public DateTime RequestedAt { get; set; }
    public string City { get; set; } = string.Empty;
    public string PeriodDescription { get; set; } = string.Empty;
}
