using weather_proj.Common;
using weather_proj.Models;
using weather_proj.Services;
using weather_proj.Validation;

namespace weather_proj.UI;

public class ConsoleMenu
{
    private readonly IWeatherService _weatherService;
    private readonly ISettingsService _settingsService;

    public ConsoleMenu(IWeatherService weatherService, ISettingsService settingsService)
    {
        _weatherService = weatherService;
        _settingsService = settingsService;
    }

    public async Task RunAsync()
    {
        bool isRunning = true;

        while (isRunning)
        {
            DisplayMenu();
            Console.Write("Выберите пункт меню: ");
            string? choice = Console.ReadLine()?.Trim();

            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await ShowForecastTomorrowAsync();
                    break;
                case "2":
                    await ShowForecastWeekAsync();
                    break;
                case "3":
                    await ShowForecastSpecificDateAsync();
                    break;
                case "4":
                    await ShowForecastRangeAsync();
                    break;
                case "5":
                    ChangeCity();
                    break;
                case "6":
                    ShowHistory();
                    break;
                case "0":
                    isRunning = false;
                    Console.WriteLine("Завершение работы программы.");
                    break;
                default:
                    PrintError("Неверный выбор. Введите число от 0 до 6.");
                    break;
            }

            if (isRunning)
            {
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }
    }

    private void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine($"   ПОГОДНЫЙ СЕРВИС | Город: {_settingsService.CurrentCity}");
        Console.WriteLine("========================================");
        Console.WriteLine("1. Прогноз на завтра");
        Console.WriteLine("2. Прогноз на ближайшие дни (до 5 дней)");
        Console.WriteLine("3. Прогноз на определенную дату");
        Console.WriteLine("4. Прогноз за диапазон дат");
        Console.WriteLine("5. Сменить текущий город");
        Console.WriteLine("6. История запросов");
        Console.WriteLine("0. Выход");
        Console.WriteLine("========================================");
    }

    private async Task ShowForecastTomorrowAsync()
    {
        string city = _settingsService.CurrentCity;
        Console.WriteLine($"Загрузка прогноза на завтра для города '{city}'...");

        var result = await _weatherService.GetForecastDayAsync(city);

        if (result.IsFailure)
        {
            PrintError(result.Error!);
            return;
        }

        _settingsService.AddEntryToHistory(new HistoryRecord
        {
            RequestedAt = DateTime.Now,
            City = city,
            PeriodDescription = "Прогноз на завтра"
        });

        PrintForecastItem(result.Value!);
    }

    private async Task ShowForecastWeekAsync()
    {
        string city = _settingsService.CurrentCity;
        Console.WriteLine($"Загрузка прогноза для города '{city}'...");

        var result = await _weatherService.GetForecastWeekAsync(city);

        if (result.IsFailure)
        {
            PrintError(result.Error!);
            return;
        }

        _settingsService.AddEntryToHistory(new HistoryRecord
        {
            RequestedAt = DateTime.Now,
            City = city,
            PeriodDescription = "Прогноз на 5 дней"
        });

        PrintForecastTable(result.Value!);
    }

    private async Task ShowForecastSpecificDateAsync()
    {
        Console.Write("Введите дату (дд.мм.гггг): ");
        string? input = Console.ReadLine();

        var dateValidationResult = DateValidator.ValidateDate(input);

        if (dateValidationResult.IsFailure)
        {
            PrintError(dateValidationResult.Error!);
            return;
        }

        DateTime targetDate = dateValidationResult.Value;
        string city = _settingsService.CurrentCity;
        Console.WriteLine($"Загрузка прогноза на {targetDate:dd.MM.yyyy} для города '{city}'...");

        var result = await _weatherService.GetForecastSpecificDateAsync(city, targetDate);

        if (result.IsFailure)
        {
            PrintError(result.Error!);
            return;
        }

        _settingsService.AddEntryToHistory(new HistoryRecord
        {
            RequestedAt = DateTime.Now,
            City = city,
            PeriodDescription = $"Прогноз на {targetDate:dd.MM.yyyy}"
        });

        PrintForecastItem(result.Value!);
    }

    private async Task ShowForecastRangeAsync()
    {
        Console.Write("Введите дату начала (дд.мм.гггг): ");
        string? startInput = Console.ReadLine();

        Console.Write("Введите дату окончания (дд.мм.гггг): ");
        string? endInput = Console.ReadLine();

        var rangeValidationResult = DateValidator.ValidateDateRange(startInput, endInput);

        if (rangeValidationResult.IsFailure)
        {
            PrintError(rangeValidationResult.Error!);
            return;
        }

        var (startDate, endDate) = rangeValidationResult.Value;
        string city = _settingsService.CurrentCity;
        Console.WriteLine($"Загрузка прогноза с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy} для города '{city}'...");

        var result = await _weatherService.GetForecastForPeriodAsync(city, startDate, endDate);

        if (result.IsFailure)
        {
            PrintError(result.Error!);
            return;
        }

        _settingsService.AddEntryToHistory(new HistoryRecord
        {
            RequestedAt = DateTime.Now,
            City = city,
            PeriodDescription = $"Период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}"
        });

        PrintForecastTable(result.Value!);
    }

    private void ChangeCity()
    {
        Console.Write("Введите новое название города (на латинице, например Warsaw, Berlin): ");
        string? newCity = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(newCity))
        {
            PrintError("Название города не может быть пустым.");
            return;
        }

        _settingsService.ChangeCurrentCity(newCity.Trim());
        Console.WriteLine($"Текущий город успешно изменен на: {_settingsService.CurrentCity}");
    }

    private void ShowHistory()
    {
        var history = _settingsService.GetAllHistoryRecords();

        if (history.Count == 0)
        {
            Console.WriteLine("История запросов пуста.");
            return;
        }

        Console.WriteLine("--- История запросов ---");
        foreach (var record in history)
        {
            Console.WriteLine($"[{record.RequestedAt:dd.MM.yyyy HH:mm:ss}] {record.City} — {record.PeriodDescription}");
        }
    }

    private static void PrintForecastItem(ForecastItem item)
    {
        Console.WriteLine($"Дата: {item.Date:dd.MM.yyyy}");
        Console.WriteLine($"Температура: от {item.MinTemp:F1}°C до {item.MaxTemp:F1}°C");
        Console.WriteLine($"Условия: {item.Description}");
    }

    private static void PrintForecastTable(List<ForecastItem> items)
    {
        Console.WriteLine($"\n{"Дата",-12} | {"Мин. t°C",-10} | {"Макс. t°C",-10} | {"Погода"}");
        Console.WriteLine(new string('-', 55));

        foreach (var item in items)
        {
            Console.WriteLine($"{item.Date:dd.MM.yyyy,-12} | {item.MinTemp,10:F1} | {item.MaxTemp,10:F1} | {item.Description}");
        }
    }

    private static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[Ошибка] {message}");
        Console.ResetColor();
    }
}