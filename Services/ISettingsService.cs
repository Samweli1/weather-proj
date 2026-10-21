using Weatherproj.Models;

namespace Weatherproj.Services;

public interface ISettingsService
{
    public string GetCurrentCity();
    public void ChangeCurrentCity(string newCity);
    public void AddEntryToHistory(HistoryRecord record);
    public List<HistoryRecord> GetAllHistoryRecords();
}