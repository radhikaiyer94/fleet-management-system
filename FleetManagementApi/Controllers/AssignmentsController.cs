using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Exceptions;
using FleetManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using FleetManagementApi.Services;
using FleetManagementApi.DTO;

namespace FleetManagementApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly AssignmentsService _assignmentsService;

    public AssignmentsController(AssignmentsService assignmentsService)
    {
        _assignmentsService = assignmentsService;
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet]
    public async Task<IActionResult> GetAssignmentsAsync([FromQuery] GetAssignmentsQuery q, CancellationToken cancellationToken = default)
    {
        var assignments = await _assignmentsService.GetAsync(q, cancellationToken);
        return Ok(assignments);
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAssignmentsAsync(CancellationToken cancellationToken = default)
    {
        var assignments = await _assignmentsService.GetActiveAsync(cancellationToken);
        return Ok(assignments);
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentsService.GetByIdAsync(id, cancellationToken);
        if (assignment == null)
            throw new NotFoundException("Assignment not found.");
        return Ok(assignment);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPost]
    public async Task<IActionResult> CreateAssignmentAsync([FromBody] AssignmentDTO assignment, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string error = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(error);
        }
        var created = await _assignmentsService.CreateAsync(assignment, cancellationToken);
        return Created($"/api/assignments/{created.Id}", created);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssignmentAsync(Guid id, [FromBody] AssignmentDTO assignmentDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string error = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(error);
        }
        await _assignmentsService.UpdateAsync(id, assignmentDto, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> CompleteAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentsService.CompleteAsync(id, cancellationToken);
        return Ok(assignment);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> CancelAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentsService.CancelAsync(id, cancellationToken);
        return Ok(assignment);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _assignmentsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
