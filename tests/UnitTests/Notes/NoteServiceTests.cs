using Habitat.BackEnd.Progress.Application.DTOs.Notes;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Application.Services.Notes;
using Habitat.BackEnd.Progress.TestSupport;

namespace UnitTests.Notes;

public sealed class NoteServiceTests
{
    [Fact]
    public async Task CreateAsync_StoresNoteDateFromRequest()
    {
        var store = CreateStore();
        var service = new NoteService(store, store);
        var noteDate = new DateOnly(2026, 5, 18);

        var result = await service.CreateAsync(1, new CreateNoteRequest { Title = "Reflexão", Content = "Hoje foi um bom dia.", Date = noteDate });

        Assert.True(result.IsSuccess);
        Assert.Equal(noteDate, result.Value!.Date);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_ForAnotherUsersNote()
    {
        var store = CreateStore();
        store.Notes.Add(new Note { Id = 10, UserId = 2, Content = "private", Date = new DateOnly(2026, 5, 18) });
        var service = new NoteService(store, store);

        var result = await service.GetByIdAsync(1, 10);

        Assert.False(result.IsSuccess);
        Assert.Equal("notes.not_found", result.Error!.Code);
    }

    private static InMemoryHabitatStore CreateStore()
    {
        var store = new InMemoryHabitatStore();
        store.Users.Add(new User { Id = 1, Name = "Daniel", Email = "daniel@email.com", IsActive = true });
        return store;
    }
}
