using System.Globalization;
using weather_proj.Common;


namespace weather_proj.Validation;

public static class DateValidator
{
    private const string DateFormat = "dd.MM.yyyy";

    public static Result<DateTime> ValidateDate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<DateTime>.Failure("Дата не может быть пустой.");
        }

        if (!DateTime.TryParseExact(input.Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return Result<DateTime>.Failure($"Неверный формат даты. Ожидается формат {DateFormat} (например, 28.10.2026).");
        }

        if (date.Date < DateTime.Today)
        {
            return Result<DateTime>.Failure("Нельзя запросить прогноз на прошедшую дату.");
        }

        if (date.Date > DateTime.Today.AddDays(4))
        {
            return Result<DateTime>.Failure("Прогноз доступен только на ближайшие 5 дней (включая сегодня).");
        }

        return Result<DateTime>.Success(date.Date);
    }

    public static Result<(DateTime Start, DateTime End)> ValidateDateRange(string? startInput, string? endInput)
    {
        var startResult = ValidateDate(startInput);
        if (startResult.IsFailure)
        {
            return Result<(DateTime Start, DateTime End)>.Failure($"Дата начала: {startResult.Error}");
        }

        var endResult = ValidateDate(endInput);
        if (endResult.IsFailure)
        {
            return Result<(DateTime Start, DateTime End)>.Failure($"Дата окончания: {endResult.Error}");
        }

        if (startResult.Value > endResult.Value)
        {
            return Result<(DateTime Start, DateTime End)>.Failure("Дата начала не может быть позже даты окончания.");
        }

        return Result<(DateTime Start, DateTime End)>.Success((startResult.Value, endResult.Value));
    }
}