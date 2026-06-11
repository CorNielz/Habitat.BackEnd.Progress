using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.Services.Dashboard;

public static class DateRangeCalculator
{
    public static (DateOnly From, DateOnly To) Resolve(DashboardPeriod period, DateOnly? from = null, DateOnly? to = null)
    {
        if (from.HasValue && to.HasValue)
        {
            return from.Value <= to.Value ? (from.Value, to.Value) : (to.Value, from.Value);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return period switch
        {
            DashboardPeriod.WEEK => (today.AddDays(-6), today),
            DashboardPeriod.YEAR => (new DateOnly(today.Year, 1, 1), today),
            _ => (new DateOnly(today.Year, today.Month, 1), today)
        };
    }
}
