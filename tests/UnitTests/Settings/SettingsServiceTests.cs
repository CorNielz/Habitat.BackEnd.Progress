using Habitat.BackEnd.Progress.Application.DTOs.Settings;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Application.Services.Settings;
using Habitat.BackEnd.Progress.TestSupport;

namespace UnitTests.Settings;

public sealed class SettingsServiceTests
{
    [Fact]
    public async Task UpdateAsync_StoresUserPreferences()
    {
        var store = new InMemoryHabitatStore();
        store.Users.Add(new User { Id = 1, Name = "Daniel", Email = "daniel@email.com", IsActive = true });
        var service = new SettingsService(store, store);

        var result = await service.UpdateAsync(1, new UpdateSettingsRequest
        {
            Theme = Theme.DARK,
            DefaultDashboardPeriod = DashboardPeriod.WEEK,
            FirstDayOfWeek = FirstDayOfWeek.SUNDAY,
            ShowHomeSummary = false
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(Theme.DARK, result.Value!.Theme);
        Assert.False(result.Value.ShowHomeSummary);
    }
}
