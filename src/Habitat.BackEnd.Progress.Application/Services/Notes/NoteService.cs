using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Notes;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.Application.Mappings;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Services.Notes;

public sealed class NoteService : INoteService
{
    private readonly INoteRepository _notes;
    private readonly IUserRepository _users;

    public NoteService(INoteRepository notes, IUserRepository users)
    {
        _notes = notes;
        _users = users;
    }

    public async Task<Result<PagedResponse<NoteResponse>>> ListAsync(int userId, PaginationRequest pagination, DateOnly? date, CancellationToken cancellationToken = default)
    {
        if (!await ActiveUserExistsAsync(userId, cancellationToken))
        {
            return Result<PagedResponse<NoteResponse>>.NotFound("users.not_found", "The authenticated user was not found.");
        }

        var page = await _notes.ListByUserAsync(userId, pagination, date, cancellationToken);
        return Result<PagedResponse<NoteResponse>>.Success(PagedResponse<NoteResponse>.Create(
            page.Items.Select(n => n.ToResponse()).ToArray(),
            page.TotalItems,
            pagination));
    }

    public async Task<Result<NoteResponse>> GetByIdAsync(int userId, int noteId, CancellationToken cancellationToken = default)
    {
        var note = await _notes.GetByIdForUserAsync(userId, noteId, cancellationToken);
        return note is null
            ? Result<NoteResponse>.NotFound("notes.not_found", "The requested note was not found.")
            : Result<NoteResponse>.Success(note.ToResponse());
    }

    public async Task<Result<NoteResponse>> CreateAsync(int userId, CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await ActiveUserExistsAsync(userId, cancellationToken))
        {
            return Result<NoteResponse>.NotFound("users.not_found", "The authenticated user was not found.");
        }

        var validation = ValidateNote(request.Content, request.Date);
        if (validation is not null)
        {
            return Result<NoteResponse>.Validation(validation.Code, validation.Message);
        }

        var now = DateTime.UtcNow;
        var note = new Note
        {
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            Content = request.Content.Trim(),
            Date = request.Date,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _notes.CreateAsync(note, cancellationToken);
        return Result<NoteResponse>.Success(created.ToResponse());
    }

    public async Task<Result<NoteResponse>> UpdateAsync(int userId, int noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var note = await _notes.GetByIdForUserAsync(userId, noteId, cancellationToken);
        if (note is null)
        {
            return Result<NoteResponse>.NotFound("notes.not_found", "The requested note was not found.");
        }

        var validation = ValidateNote(request.Content, request.Date);
        if (validation is not null)
        {
            return Result<NoteResponse>.Validation(validation.Code, validation.Message);
        }

        note.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
        note.Content = request.Content.Trim();
        note.Date = request.Date;
        note.UpdatedAt = DateTime.UtcNow;

        var updated = await _notes.UpdateAsync(note, cancellationToken);
        return updated
            ? Result<NoteResponse>.Success(note.ToResponse())
            : Result<NoteResponse>.NotFound("notes.not_found", "The requested note was not found.");
    }

    public async Task<Result> DeleteAsync(int userId, int noteId, CancellationToken cancellationToken = default)
    {
        var deleted = await _notes.DeleteAsync(userId, noteId, cancellationToken);
        return deleted
            ? Result.Success()
            : Result.NotFound("notes.not_found", "The requested note was not found.");
    }

    private async Task<bool> ActiveUserExistsAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user is not null && user.IsActive;
    }

    private static Error? ValidateNote(string content, DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new Error("notes.invalid_content", "Note content is required.");
        }

        if (date == default)
        {
            return new Error("notes.invalid_date", "Note date is required.");
        }

        return null;
    }
}
