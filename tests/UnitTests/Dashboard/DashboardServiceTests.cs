using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Application.Services.Dashboard;
using Habitat.BackEnd.Progress.TestSupport;

namespace UnitTests.Dashboard;

public sealed class DashboardServiceTests
{
    [Fact]
    public void HabitScheduleCalculator_CountsDailyOccurrences()
    {
        var habit = new Habit { StartDate = new DateOnly(2026, 5, 1), FrequencyType = HabitFrequencyType.DAILY, FrequencyValue = "1" };

        var count = HabitScheduleCalculator.CountExpectedOccurrences(habit, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 3));

        Assert.Equal(3, count);
    }

    [Fact]
    public void HabitScheduleCalculator_CountsWeeklyNamedDays()
    {
        var habit = new Habit { StartDate = new DateOnly(2026, 5, 1), FrequencyType = HabitFrequencyType.WEEKLY, FrequencyValue = "MONDAY,WEDNESDAY" };

        var count = HabitScheduleCalculator.CountExpectedOccurrences(habit, new DateOnly(2026, 5, 4), new DateOnly(2026, 5, 10));

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetSummaryAsync_UsesOnlyAuthenticatedUsersData()
    {
        var store = new InMemoryHabitatStore();
        store.Users.Add(new User { Id = 1, Name = "Daniel", Email = "daniel@email.com", IsActive = true });
        store.Habits.Add(new Habit { Id = 1, UserId = 1, Title = "Ler", FrequencyType = HabitFrequencyType.DAILY, FrequencyValue = "1", StartDate = DateOnly.FromDateTime(DateTime.UtcNow), IsActive = true });
        store.Habits.Add(new Habit { Id = 2, UserId = 2, Title = "Outro", FrequencyType = HabitFrequencyType.DAILY, FrequencyValue = "1", StartDate = DateOnly.FromDateTime(DateTime.UtcNow), IsActive = true });
        store.Records.Add(new HabitRecord { Id = 1, HabitId = 1, RecordDate = DateOnly.FromDateTime(DateTime.UtcNow), Completed = true, RecordedAt = DateTime.UtcNow });
        store.Records.Add(new HabitRecord { Id = 2, HabitId = 2, RecordDate = DateOnly.FromDateTime(DateTime.UtcNow), Completed = true, RecordedAt = DateTime.UtcNow });
        var service = new DashboardService(store, store, store, store);

        var result = await service.GetSummaryAsync(1, DashboardPeriod.WEEK);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalHabits);
        Assert.Equal(1, result.Value.CompletedToday);
    }
}
