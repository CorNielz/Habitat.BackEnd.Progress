using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Habits;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Application.Services.Habits;
using Habitat.BackEnd.Progress.TestSupport;

namespace UnitTests.Habits;

public sealed class HabitServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesHabitForAuthenticatedUser()
    {
        var store = CreateStoreWithUser();
        var service = new HabitService(store, store);

        var result = await service.CreateAsync(1, new CreateHabitRequest
        {
            Title = "Ler",
            Description = "Leitura diária",
            FrequencyType = HabitFrequencyType.DAILY,
            FrequencyValue = "1",
            StartDate = new DateOnly(2026, 5, 1)
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Ler", result.Value!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_WhenHabitBelongsToAnotherUser()
    {
        var store = CreateStoreWithUser();
        store.Users.Add(new User { Id = 2, Name = "Other", Email = "other@email.com", IsActive = true });
        store.Habits.Add(new Habit { Id = 5, UserId = 2, Title = "Privado", FrequencyType = HabitFrequencyType.DAILY, FrequencyValue = "1", StartDate = new DateOnly(2026, 5, 1) });
        var service = new HabitService(store, store);

        var result = await service.GetByIdAsync(1, 5);

        Assert.False(result.IsSuccess);
        Assert.Equal("habits.not_found", result.Error!.Code);
    }

    [Fact]
    public async Task DeleteAsync_RemovesHabitAndRecords()
    {
        var store = CreateStoreWithUser();
        var service = new HabitService(store, store);
        var created = await service.CreateAsync(1, new CreateHabitRequest { Title = "Ler", FrequencyType = HabitFrequencyType.DAILY, FrequencyValue = "1", StartDate = new DateOnly(2026, 5, 1) });

        var result = await service.DeleteAsync(1, created.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(store.Habits);
    }

    private static InMemoryHabitatStore CreateStoreWithUser()
    {
        var store = new InMemoryHabitatStore();
        store.Users.Add(new User { Id = 1, Name = "Daniel", Email = "daniel@email.com", Role = UserRole.USER, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        return store;
    }
}
