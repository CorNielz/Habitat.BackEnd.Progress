using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Notes;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Habitat.BackEnd.Progress.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notes")]
public sealed class NotesController : ControllerBase
{
    private readonly INoteService _notes;

    public NotesController(INoteService notes)
    {
        _notes = notes;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<NoteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] DateOnly? date = null, CancellationToken cancellationToken = default)
    {
        var result = await _notes.ListAsync(User.GetUserId(), new PaginationRequest(page, pageSize), date, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _notes.GetByIdAsync(User.GetUserId(), id, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateNoteRequest request, CancellationToken cancellationToken)
    {
        var result = await _notes.CreateAsync(User.GetUserId(), request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.ToProblem(this);
        }

        return Created($"/api/v1/notes/{result.Value!.Id}", result.Value);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var result = await _notes.UpdateAsync(User.GetUserId(), id, request, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _notes.DeleteAsync(User.GetUserId(), id, cancellationToken);
        return result.ToNoContentOrProblem(this);
    }
}
