using System.Globalization;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Services.Dashboard;

public static class HabitScheduleCalculator
{
    public static int CountExpectedOccurrences(Habit habit, DateOnly from, DateOnly to)
    {
        var effectiveFrom = habit.StartDate > from ? habit.StartDate : from;
        if (effectiveFrom > to)
        {
            return 0;
        }

        return habit.FrequencyType switch
        {
            HabitFrequencyType.DAILY => CountDaysInclusive(effectiveFrom, to),
            HabitFrequencyType.WEEKLY => CountWeeklyOccurrences(habit.FrequencyValue, effectiveFrom, to),
            HabitFrequencyType.MONTHLY => CountMonthlyOccurrences(habit.FrequencyValue, effectiveFrom, to),
            HabitFrequencyType.CUSTOM => CountDaysInclusive(effectiveFrom, to),
            _ => 0
        };
    }

    private static int CountDaysInclusive(DateOnly from, DateOnly to) => to.DayNumber - from.DayNumber + 1;

    private static int CountWeeklyOccurrences(string frequencyValue, DateOnly from, DateOnly to)
    {
        var weekdays = ParseWeekdays(frequencyValue);
        if (weekdays.Count > 0)
        {
            var count = 0;
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (weekdays.Contains(date.DayOfWeek))
                {
                    count++;
                }
            }

            return count;
        }

        var timesPerWeek = ParseLeadingPositiveInt(frequencyValue) ?? 1;
        var days = CountDaysInclusive(from, to);
        return (int)Math.Ceiling(days / 7d * timesPerWeek);
    }

    private static int CountMonthlyOccurrences(string frequencyValue, DateOnly from, DateOnly to)
    {
        var dayOfMonth = ParseLeadingPositiveInt(frequencyValue);
        if (dayOfMonth is >= 1 and <= 31)
        {
            var count = 0;
            var cursor = new DateOnly(from.Year, from.Month, 1);
            var end = new DateOnly(to.Year, to.Month, 1);
            while (cursor <= end)
            {
                var day = Math.Min(dayOfMonth.Value, DateTime.DaysInMonth(cursor.Year, cursor.Month));
                var occurrence = new DateOnly(cursor.Year, cursor.Month, day);
                if (occurrence >= from && occurrence <= to)
                {
                    count++;
                }

                cursor = cursor.AddMonths(1);
            }

            return count;
        }

        return ((to.Year - from.Year) * 12) + to.Month - from.Month + 1;
    }

    private static HashSet<DayOfWeek> ParseWeekdays(string value)
    {
        var weekdays = new HashSet<DayOfWeek>();
        var tokens = value.Split(',', ';', '|', ' ', (char)(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        foreach (var token in tokens)
        {
            if (Enum.TryParse<DayOfWeek>(token, ignoreCase: true, out var weekday))
            {
                weekdays.Add(weekday);
            }
        }

        return weekdays;
    }

    private static int? ParseLeadingPositiveInt(string value)
    {
        var digits = new string(value.Trim().TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : null;
    }
}
