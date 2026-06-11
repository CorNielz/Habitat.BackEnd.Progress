using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.HabitRecords;
using Habitat.BackEnd.Progress.Application.DTOs.Habits;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Habitat.BackEnd.Progress.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/habits")]
public sealed class HabitsController : ControllerBase
{
    private readonly IHabitService _habits;
    private readonly IHabitRecordService _records;

    public HabitsController(IHabitService habits, IHabitRecordService records)
    {
        _habits = habits;
        _records = records;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<HabitResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _habits.ListAsync(User.GetUserId(), new PaginationRequest(page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(HabitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _habits.GetByIdAsync(User.GetUserId(), id, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(HabitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateHabitRequest request, CancellationToken cancellationToken)
    {
        var result = await _habits.CreateAsync(User.GetUserId(), request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.ToProblem(this);
        }

        return Created($"/api/v1/habits/{result.Value!.Id}", result.Value);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(HabitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateHabitRequest request, CancellationToken cancellationToken)
    {
        var result = await _habits.UpdateAsync(User.GetUserId(), id, request, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _habits.DeleteAsync(User.GetUserId(), id, cancellationToken);
        return result.ToNoContentOrProblem(this);
    }

    [HttpGet("{habitId:int}/records")]
    [ProducesResponseType(typeof(PagedResponse<HabitRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListRecords([FromRoute] int habitId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        var result = await _records.ListAsync(User.GetUserId(), habitId, new PaginationRequest(page, pageSize), from, to, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{habitId:int}/records")]
    [ProducesResponseType(typeof(HabitRecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRecord([FromRoute] int habitId, [FromBody] CreateHabitRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _records.CreateAsync(User.GetUserId(), habitId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.ToProblem(this);
        }

        return Created($"/api/v1/habits/{habitId}/records", result.Value);
    }

    [HttpDelete("{habitId:int}/records")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecordByDate([FromRoute] int habitId, [FromQuery] DateOnly recordDate, CancellationToken cancellationToken)
    {
        var result = await _records.DeleteByDateAsync(User.GetUserId(), habitId, recordDate, cancellationToken);
        return result.ToNoContentOrProblem(this);
    }
}
