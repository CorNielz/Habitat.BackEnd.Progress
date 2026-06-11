using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Notes;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Services;

public interface INoteService
{
    Task<Result<PagedResponse<NoteResponse>>> ListAsync(int userId, PaginationRequest pagination, DateOnly? date, CancellationToken cancellationToken = default);
    Task<Result<NoteResponse>> GetByIdAsync(int userId, int noteId, CancellationToken cancellationToken = default);
    Task<Result<NoteResponse>> CreateAsync(int userId, CreateNoteRequest request, CancellationToken cancellationToken = default);
    Task<Result<NoteResponse>> UpdateAsync(int userId, int noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int noteId, CancellationToken cancellationToken = default);
}
