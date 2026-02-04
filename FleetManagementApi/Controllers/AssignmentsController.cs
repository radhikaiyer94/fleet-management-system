using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Domain.Enums;
using FleetManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly FleetDbContext _dbContext;

    public AssignmentsController(FleetDbContext context)
    {
        _dbContext = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignmentsAsync([FromQuery] GetAssignmentsQuery q, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Assignments.Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(a => a.Notes != null && a.Notes.ToLower().Contains(term));
        }
        if (q.VehicleId.HasValue)
            query = query.Where(a => a.VehicleId == q.VehicleId.Value);
        if (q.DriverId.HasValue)
            query = query.Where(a => a.DriverId == q.DriverId.Value);
        if (q.Status.HasValue)
            query = query.Where(a => a.Status == q.Status.Value);

        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            var sortByLower = q.SortBy.Trim().ToLower();
            var desc = string.Equals(q.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            query = sortByLower switch
            {
                "id" => desc ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id),
                "vehicleid" => desc ? query.OrderByDescending(a => a.VehicleId) : query.OrderBy(a => a.VehicleId),
                "driverid" => desc ? query.OrderByDescending(a => a.DriverId) : query.OrderBy(a => a.DriverId),
                "startdate" => desc ? query.OrderByDescending(a => a.StartDate) : query.OrderBy(a => a.StartDate),
                "enddate" => desc ? query.OrderByDescending(a => a.EndDate) : query.OrderBy(a => a.EndDate),
                "status" => desc ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                "createdat" => desc ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
                _ => query.OrderBy(a => a.Id)
            };
        }
        else
            query = query.OrderBy(a => a.Id);

        if (q.Page > 0 && q.PageSize > 0)
            query = query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize);

        var assignments = await query.ToListAsync(cancellationToken);
        return Ok(assignments);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAssignmentsAsync(CancellationToken cancellationToken = default)
    {
        var activeAssignments = await _dbContext.Assignments
            .Where(a => !a.IsDeleted && a.Status == AssignmentStatus.Active)
            .ToListAsync(cancellationToken);
        return Ok(activeAssignments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _dbContext.Assignments.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == id, cancellationToken);
        return assignment != null ? Ok(assignment) : NotFound(new { message = "Assignment not found." });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAssignmentAsync([FromBody] Assignment assignment, CancellationToken cancellationToken = default)
    {
        var vehicleExists = await _dbContext.Vehicles.AnyAsync(v => v.Id == assignment.VehicleId && !v.IsDeleted, cancellationToken);
        if (!vehicleExists)
        {
            return BadRequest(new { message = "Vehicle not found." });
        }

        var driverExists = await _dbContext.Drivers.AnyAsync(d => d.Id == assignment.DriverId && !d.IsDeleted, cancellationToken);
        if (!driverExists)
        {
            return BadRequest(new { message = "Driver not found." });
        }

        var errors = await ValidateAssignmentAsync(assignment, null, cancellationToken);
        if (errors.Count > 0)
        {
            return BadRequest(new { message = string.Join(", ", errors) });
        }

        if (assignment.Id == default) assignment.Id = Guid.NewGuid();
        assignment.CreatedAt = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.Assignments.AddAsync(assignment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetAssignmentAsync), new { id = assignment.Id }, assignment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssignmentAsync(Guid id, [FromBody] Assignment assignment, CancellationToken cancellationToken = default)
    {
        if (id != assignment.Id)
        {
            return BadRequest(new { message = "Assignment ID mismatch." });
        }

        var existingAssignment = await _dbContext.Assignments.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == id, cancellationToken);
        if (existingAssignment == null)
        {
            return NotFound(new { message = "Assignment not found." });
        }

        var errors = await ValidateAssignmentAsync(assignment, existingAssignment, cancellationToken);
        if (errors.Count > 0)
        {
            return BadRequest(new { message = string.Join(", ", errors) });
        }

        existingAssignment.VehicleId = assignment.VehicleId;
        existingAssignment.DriverId = assignment.DriverId;
        existingAssignment.StartDate = assignment.StartDate;
        existingAssignment.EndDate = assignment.EndDate;
        existingAssignment.Status = assignment.Status;
        existingAssignment.Notes = assignment.Notes;
        existingAssignment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> CompleteAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _dbContext.Assignments.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == id, cancellationToken);
        if (assignment == null)
        {
            return NotFound(new { message = "Assignment not found." });
        }

        assignment.Status = AssignmentStatus.Completed;
        assignment.EndDate = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(assignment);
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> CancelAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _dbContext.Assignments.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == id, cancellationToken);
        if (assignment == null)
        {
            return NotFound(new { message = "Assignment not found." });
        }

        assignment.Status = AssignmentStatus.Cancelled;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(assignment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingAssignment = await _dbContext.Assignments.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == id, cancellationToken);
        if (existingAssignment == null)
        {
            return NotFound(new { message = "Assignment not found." });
        }

        existingAssignment.IsDeleted = true;
        existingAssignment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<List<string>> ValidateAssignmentAsync(Assignment assignment, Assignment? existingAssignment, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (assignment.StartDate > assignment.EndDate)
        {
            errors.Add("Start date cannot be after end date.");
        }

        if (existingAssignment == null && assignment.StartDate < DateTime.UtcNow)
        {
            errors.Add("Start date cannot be in the past.");
        }

        if (existingAssignment == null && assignment.EndDate < DateTime.UtcNow)
        {
            errors.Add("End date cannot be in the past.");
        }

        var vehicleHasActiveAssignment = await _dbContext.Assignments
            .AnyAsync(a => a.VehicleId == assignment.VehicleId && a.Status == AssignmentStatus.Active && !a.IsDeleted && (existingAssignment == null || a.Id != existingAssignment.Id), cancellationToken);
        if (vehicleHasActiveAssignment)
        {
            errors.Add("Vehicle already has an active assignment.");
        }

        var driverHasActiveAssignment = await _dbContext.Assignments
            .AnyAsync(a => a.DriverId == assignment.DriverId && a.Status == AssignmentStatus.Active && !a.IsDeleted && (existingAssignment == null || a.Id != existingAssignment.Id), cancellationToken);
        if (driverHasActiveAssignment)
        {
            errors.Add("Driver already has an active assignment.");
        }

        if (existingAssignment != null && assignment.VehicleId != existingAssignment.VehicleId)
        {
            var vehicleExists = await _dbContext.Vehicles.AnyAsync(v => v.Id == assignment.VehicleId && !v.IsDeleted, cancellationToken);
            if (!vehicleExists)
            {
                errors.Add("Vehicle not found.");
            }
        }

        if (existingAssignment != null && assignment.DriverId != existingAssignment.DriverId)
        {
            var driverExists = await _dbContext.Drivers.AnyAsync(d => d.Id == assignment.DriverId && !d.IsDeleted, cancellationToken);
            if (!driverExists)
            {
                errors.Add("Driver not found.");
            }
        }

        return errors;
    }
}
