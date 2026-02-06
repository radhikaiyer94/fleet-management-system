using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Exceptions;
using FleetManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly FleetDbContext _dbContext;

    public DriversController(FleetDbContext context)
    {
        _dbContext = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetDriversAsync([FromQuery] GetDriversQuery q, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Drivers.Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(d =>
                (d.FirstName != null && d.FirstName.ToLower().Contains(term)) ||
                (d.LastName != null && d.LastName.ToLower().Contains(term)) ||
                (d.Email != null && d.Email.ToLower().Contains(term)) ||
                (d.PhoneNumber != null && d.PhoneNumber.ToLower().Contains(term)) ||
                (d.LicenseNumber != null && d.LicenseNumber.ToLower().Contains(term)));
        }
        if (q.Status.HasValue)
            query = query.Where(d => d.Status == q.Status.Value);

        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            var sortByLower = q.SortBy.Trim().ToLower();
            var desc = string.Equals(q.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            query = sortByLower switch
            {
                "id" => desc ? query.OrderByDescending(d => d.Id) : query.OrderBy(d => d.Id),
                "firstname" => desc ? query.OrderByDescending(d => d.FirstName) : query.OrderBy(d => d.FirstName),
                "lastname" => desc ? query.OrderByDescending(d => d.LastName) : query.OrderBy(d => d.LastName),
                "status" => desc ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
                "createdat" => desc ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
                _ => query.OrderBy(d => d.Id)
            };
        }
        else
            query = query.OrderBy(d => d.Id);

        if (q.Page > 0 && q.PageSize > 0)
            query = query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize);

        var drivers = await query.ToListAsync(cancellationToken);
        return Ok(drivers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDriverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => !d.IsDeleted && d.Id == id, cancellationToken);
        if (driver == null)
        {
            throw new NotFoundException("Driver not found.");
        }
        return Ok(driver);
    }

    [HttpGet("{id}/assignments")]
    public async Task<IActionResult> GetDriverAssignmentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingDriver = await _dbContext.Drivers.FirstOrDefaultAsync(d => !d.IsDeleted && d.Id == id, cancellationToken);
        if (existingDriver == null)
        {
            throw new NotFoundException("Driver not found.");
        }

        var assignments = await _dbContext.Assignments.Where(a => a.DriverId == id && !a.IsDeleted).ToListAsync(cancellationToken);
        if (assignments.Count == 0)
            throw new NotFoundException("No assignments found for this driver.");
        return Ok(assignments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDriverAsync([FromBody] Driver driver, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(errors);
        }
        if (driver.Id == default) driver.Id = Guid.NewGuid();
        driver.CreatedAt = DateTime.UtcNow;
        driver.UpdatedAt = DateTime.UtcNow;

        await _dbContext.Drivers.AddAsync(driver, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetDriverAsync), new { id = driver.Id }, driver);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDriverAsync(Guid id, [FromBody] Driver driver, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(errors);
        }
        if (id != driver.Id)
            throw new BadRequestException("Driver ID mismatch.");

        var existingDriver = await _dbContext.Drivers.FirstOrDefaultAsync(d => !d.IsDeleted && d.Id == id, cancellationToken);
        if (existingDriver == null)
            throw new NotFoundException("Driver not found.");

        // Idempotency: if request body matches current entity, no-op and return 204 (no DB write).
        if (DriverDataEquals(existingDriver, driver))
        {
            return NoContent();
        }

        existingDriver.FirstName = driver.FirstName;
        existingDriver.LastName = driver.LastName;
        existingDriver.Email = driver.Email;
        existingDriver.PhoneNumber = driver.PhoneNumber;
        existingDriver.LicenseNumber = driver.LicenseNumber;
        existingDriver.LicenseExpiryDate = driver.LicenseExpiryDate;
        existingDriver.DateOfEmployment = driver.DateOfEmployment;
        existingDriver.Status = driver.Status;
        existingDriver.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Compares updatable fields only (excludes Id, CreatedAt, UpdatedAt, IsDeleted, navigation properties).
    /// Used to make PUT idempotent when there is no change in data.
    /// </summary>
    private static bool DriverDataEquals(Driver existing, Driver incoming) =>
        existing.FirstName == incoming.FirstName &&
        existing.LastName == incoming.LastName &&
        existing.Email == incoming.Email &&
        existing.PhoneNumber == incoming.PhoneNumber &&
        existing.LicenseNumber == incoming.LicenseNumber &&
        existing.LicenseExpiryDate == incoming.LicenseExpiryDate &&
        existing.DateOfEmployment == incoming.DateOfEmployment &&
        existing.Status == incoming.Status;

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDriverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingDriver = await _dbContext.Drivers.FirstOrDefaultAsync(d => !d.IsDeleted && d.Id == id, cancellationToken);
        if (existingDriver == null)
        {
            throw new NotFoundException("Driver not found.");
        }

        existingDriver.IsDeleted = true;
        existingDriver.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
