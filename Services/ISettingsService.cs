using Weatherproj.Models;

namespace Weatherproj.Services;

public interface ISettingsService
{
    string GetCurrentCity();
    void ChangeCurrentCity(string newCity);
    void AddEntryToHistory(HistoryRecord record);
    List<HistoryRecord> GetAllHistoryRecords();
}