
using weather_proj.Models;

namespace weather_proj.Services;

public class SettingsService : ISettingsService
{
    private string _currentCity = "Minsk";
    private List<HistoryRecord> _history = new();

    public string GetCurrentCity()
    {
        return _currentCity;
    }

    public void ChangeCurrentCity(string newCity)
    {
        if (!string.IsNullOrEmpty(newCity))
        {
            _currentCity = newCity;
        }
        
    }

    public void AddEntryToHistory(HistoryRecord record)
    {
        _history.Add(record);
    }

    public List<HistoryRecord> GetAllHistoryRecords()
    {
        return _history;
    }
    
}

