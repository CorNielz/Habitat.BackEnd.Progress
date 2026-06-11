using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Dashboard;
using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<Result<DashboardSummaryResponse>> GetSummaryAsync(int userId, DashboardPeriod period, CancellationToken cancellationToken = default);
    Task<Result<DashboardHistoryResponse>> GetHistoryAsync(int userId, DashboardPeriod period, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}
