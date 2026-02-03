using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Domain.Enums;

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
    public IActionResult GetAssignments()
    {
        var assignments = _dbContext.Assignments.Where(assignment => !assignment.IsDeleted).ToList();
        return Ok(assignments);
    }

    [HttpGet("active")]
    public IActionResult GetActiveAssignments()
    {
        var activeAssignments = _dbContext.Assignments
            .Where(assignment => !assignment.IsDeleted && assignment.Status == AssignmentStatus.Active)
            .ToList();
        return Ok(activeAssignments);
    }

    [HttpGet("{id}")]
    public IActionResult GetAssignment(Guid id)
    {
        var assignment = _dbContext.Assignments.Where(a => !a.IsDeleted).FirstOrDefault(a => a.Id == id);
        return assignment != null ? Ok(assignment) : NotFound(new { message = "Assignment not found." });
    }

    [HttpPost]
    public IActionResult CreateAssignment([FromBody] Assignment assignment)
    {
        // Validate that the vehicle exists
        var vehicleExists = _dbContext.Vehicles.Any(v => v.Id == assignment.VehicleId && !v.IsDeleted);
        if (!vehicleExists)
        {
            return BadRequest(new { message = "Vehicle not found." });
        }

        // Validate that the driver exists
        var driverExists = _dbContext.Drivers.Any(d => d.Id == assignment.DriverId && !d.IsDeleted);
        if (!driverExists)
        {
            return BadRequest(new { message = "Driver not found." });
        }

        var errors = ValidateAssignment(assignment);
        if (errors.Count > 0)
        {
            return BadRequest(new { message = string.Join(", ", errors) });
        }

        if (assignment.Id == default) assignment.Id = Guid.NewGuid();
        assignment.CreatedAt = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        _dbContext.Assignments.Add(assignment);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, assignment);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateAssignment(Guid id, [FromBody] Assignment assignment)
    {
        if (id != assignment.Id)
        {
            return BadRequest(new { message = "Assignment ID mismatch." });
        }

        var existingAssignment = _dbContext.Assignments.Where(a => !a.IsDeleted).FirstOrDefault(a => a.Id == id);
        if (existingAssignment == null)
        {
            return NotFound(new { message = "Assignment not found." });
        }

        var errors = ValidateAssignment(assignment, existingAssignment);
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

        _dbContext.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public IActionResult CompleteAssignment(Guid id)
    {
        var assignment = _dbContext.Assignments.Where(a => !a.IsDeleted).FirstOrDefault(a => a.Id == id);
        if (assignment == null)
        {
            return NotFound(new { message = "Assignment not found." });
        }

        assignment.Status = AssignmentStatus.Completed;
        assignment.EndDate = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return Ok(assignment);
    }

    [HttpPatch("{id}/cancel")]
    public IActionResult CancelAssignment(Guid id)
    {
        var assignment = _dbContext.Assignments.Where(a => !a.IsDeleted).FirstOrDefault(a => a.Id == id);
        if (assignment == null)
        {
            return NotFound(new { message = "Assignment not found." });
        }

        assignment.Status = AssignmentStatus.Cancelled;
        assignment.UpdatedAt = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return Ok(assignment);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteAssignment(Guid id)
    {
        var existingAssignment = _dbContext.Assignments.Where(a => !a.IsDeleted).FirstOrDefault(a => a.Id == id);
        if (existingAssignment == null)
        {
            return NotFound(new { message = "Assignment not found." });
        }

        existingAssignment.IsDeleted = true;
        existingAssignment.UpdatedAt = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return NoContent();
    }

    private List<string> ValidateAssignment(Assignment assignment, Assignment? existingAssignment = null)
    {
        List<string> errors = new List<string>();

        // Validate start and end date
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

        // Business Rule: Check if vehicle already has an active assignment
        var vehicleHasActiveAssignment = _dbContext.Assignments
            .Any(a => a.VehicleId == assignment.VehicleId && a.Status == AssignmentStatus.Active && !a.IsDeleted && (existingAssignment == null || a.Id != existingAssignment.Id));
        if (vehicleHasActiveAssignment)
        {
            errors.Add("Vehicle already has an active assignment.");
        }

        // Business Rule: Check if driver already has an active assignment
        var driverHasActiveAssignment = _dbContext.Assignments
            .Any(a => a.DriverId == assignment.DriverId && a.Status == AssignmentStatus.Active && !a.IsDeleted && (existingAssignment == null || a.Id != existingAssignment.Id));
        if (driverHasActiveAssignment)
        {
            errors.Add("Driver already has an active assignment.");
        }

        // Validate vehicle if VehicleId is changing
        if (existingAssignment != null && assignment.VehicleId != existingAssignment.VehicleId)
        {
            var vehicleExists = _dbContext.Vehicles.Any(v => v.Id == assignment.VehicleId && !v.IsDeleted);
            if (!vehicleExists)
            {
                errors.Add("Vehicle not found.");
            }
        }

        // Validate driver if DriverId is changing
        if (existingAssignment != null && assignment.DriverId != existingAssignment.DriverId)
        {
            var driverExists = _dbContext.Drivers.Any(d => d.Id == assignment.DriverId && !d.IsDeleted);
            if (!driverExists)
            {
                errors.Add("Driver not found.");
            }
        }

        return errors;
    }
}
