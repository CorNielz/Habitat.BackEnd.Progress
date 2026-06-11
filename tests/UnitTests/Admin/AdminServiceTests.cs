using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Admin;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Application.Services.Admin;
using Habitat.BackEnd.Progress.TestSupport;

namespace UnitTests.Admin;

public sealed class AdminServiceTests
{
    [Fact]
    public async Task UpdateUserRoleAsync_ChangesRoleToAdmin()
    {
        var store = new InMemoryHabitatStore();
        store.Users.Add(new User { Id = 1, Name = "Daniel", Email = "daniel@email.com", Role = UserRole.USER, IsActive = true });
        var service = new AdminService(store);

        var result = await service.UpdateUserRoleAsync(1, new UpdateUserRoleRequest { Role = UserRole.ADMIN });

        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.ADMIN, store.Users.Single().Role);
    }

    [Fact]
    public async Task ListUsersAsync_ReturnsPagedUsers()
    {
        var store = new InMemoryHabitatStore();
        store.Users.Add(new User { Id = 1, Name = "Daniel", Email = "daniel@email.com", Role = UserRole.USER, IsActive = true });
        var service = new AdminService(store);

        var result = await service.ListUsersAsync(new PaginationRequest(1, 20));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
    }
}
