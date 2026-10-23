using weather_proj.Models;

namespace weather_proj.Services;

public interface ISettingsService
{
    public string CurrentCity { get; }
    public void ChangeCurrentCity(string newCity);
    public void AddEntryToHistory(HistoryRecord record);
    public List<HistoryRecord> GetAllHistoryRecords();
}