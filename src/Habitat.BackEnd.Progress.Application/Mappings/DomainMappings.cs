using Habitat.BackEnd.Progress.Application.DTOs.HabitRecords;
using Habitat.BackEnd.Progress.Application.DTOs.Habits;
using Habitat.BackEnd.Progress.Application.DTOs.Notes;
using Habitat.BackEnd.Progress.Application.DTOs.Settings;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Mappings;

public static class DomainMappings
{
    public static UserSettingsResponse ToResponse(this UserSettings settings) => new()
    {
        Theme = settings.Theme,
        DefaultDashboardPeriod = settings.DefaultDashboardPeriod,
        FirstDayOfWeek = settings.FirstDayOfWeek,
        ShowHomeSummary = settings.ShowHomeSummary,
        UpdatedAt = settings.UpdatedAt
    };

    public static HabitResponse ToResponse(this Habit habit) => new()
    {
        Id = habit.Id,
        Title = habit.Title,
        Description = habit.Description,
        FrequencyType = habit.FrequencyType,
        FrequencyValue = habit.FrequencyValue,
        StartDate = habit.StartDate,
        CreatedAt = habit.CreatedAt,
        UpdatedAt = habit.UpdatedAt
    };

    public static HabitRecordResponse ToResponse(this HabitRecord record) => new()
    {
        Id = record.Id,
        HabitId = record.HabitId,
        RecordDate = record.RecordDate,
        Note = record.Note,
        RecordedAt = record.RecordedAt
    };

    public static NoteResponse ToResponse(this Note note) => new()
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        Date = note.Date,
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt
    };
}
