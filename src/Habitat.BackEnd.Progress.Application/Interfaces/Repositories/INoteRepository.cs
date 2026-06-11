using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Repositories;

public interface INoteRepository
{
    Task<PagedResponse<Note>> ListByUserAsync(int userId, PaginationRequest pagination, DateOnly? date, CancellationToken cancellationToken = default);
    Task<int> CountByUserBetweenAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<Note?> GetByIdForUserAsync(int userId, int noteId, CancellationToken cancellationToken = default);
    Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Note note, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int userId, int noteId, CancellationToken cancellationToken = default);
}
