using Habitat.BackEnd.Progress.Application.DTOs.HabitRecords;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Application.Services.HabitRecords;
using Habitat.BackEnd.Progress.TestSupport;

namespace UnitTests.HabitRecords;

public sealed class HabitRecordServiceTests
{
    [Fact]
    public async Task CreateAsync_ReturnsConflict_WhenRecordAlreadyExistsForDate()
    {
        var store = CreateStore();
        var service = new HabitRecordService(store, store);
        var date = new DateOnly(2026, 5, 18);
        await service.CreateAsync(1, 1, new CreateHabitRecordRequest { RecordDate = date });

        var result = await service.CreateAsync(1, 1, new CreateHabitRecordRequest { RecordDate = date });

        Assert.False(result.IsSuccess);
        Assert.Equal("records.already_exists", result.Error!.Code);
    }

    [Fact]
    public async Task DeleteByDateAsync_RemovesCompletionRecord()
    {
        var store = CreateStore();
        var service = new HabitRecordService(store, store);
        var date = new DateOnly(2026, 5, 18);
        await service.CreateAsync(1, 1, new CreateHabitRecordRequest { RecordDate = date });

        var result = await service.DeleteByDateAsync(1, 1, date);

        Assert.True(result.IsSuccess);
        Assert.Empty(store.Records);
    }

    private static InMemoryHabitatStore CreateStore()
    {
        var store = new InMemoryHabitatStore();
        store.Users.Add(new User { Id = 1, Name = "Daniel", Email = "daniel@email.com", IsActive = true });
        store.Habits.Add(new Habit { Id = 1, UserId = 1, Title = "Ler", FrequencyType = HabitFrequencyType.DAILY, FrequencyValue = "1", StartDate = new DateOnly(2026, 5, 1), IsActive = true });
        return store;
    }
}
