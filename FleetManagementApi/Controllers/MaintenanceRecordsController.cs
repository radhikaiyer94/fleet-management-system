using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Exceptions;
using FleetManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FleetManagementApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MaintenanceRecordsController : ControllerBase
{
    private readonly FleetDbContext _dbContext;

    public MaintenanceRecordsController(FleetDbContext context)
    {
        _dbContext = context;
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet]
    public async Task<IActionResult> GetMaintenanceRecordsAsync([FromQuery] GetMaintenanceRecordsQuery q, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MaintenanceRecords.Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(r =>
                (r.Description != null && r.Description.ToLower().Contains(term)) ||
                (r.ServiceProvider != null && r.ServiceProvider.ToLower().Contains(term)));
        }
        if (q.VehicleId.HasValue)
            query = query.Where(r => r.VehicleId == q.VehicleId.Value);
        if (q.Type.HasValue)
            query = query.Where(r => r.MaintenanceType == q.Type.Value);

        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            var sortByLower = q.SortBy.Trim().ToLower();
            var desc = string.Equals(q.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            query = sortByLower switch
            {
                "id" => desc ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id),
                "vehicleid" => desc ? query.OrderByDescending(r => r.VehicleId) : query.OrderBy(r => r.VehicleId),
                "maintenancetype" => desc ? query.OrderByDescending(r => r.MaintenanceType) : query.OrderBy(r => r.MaintenanceType),
                "cost" => desc ? query.OrderByDescending(r => r.Cost) : query.OrderBy(r => r.Cost),
                "servicedate" => desc ? query.OrderByDescending(r => r.ServiceDate) : query.OrderBy(r => r.ServiceDate),
                "createdat" => desc ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
                _ => query.OrderBy(r => r.Id)
            };
        }
        else
            query = query.OrderBy(r => r.Id);

        if (q.Page > 0 && q.PageSize > 0)
            query = query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize);

        var records = await query.ToListAsync(cancellationToken);
        return Ok(records);
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMaintenanceRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.MaintenanceRecords.FirstOrDefaultAsync(r => !r.IsDeleted && r.Id == id, cancellationToken);
        if (record == null)
            throw new NotFoundException("Maintenance record not found.");
        return Ok(record);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPost]
    public async Task<IActionResult> CreateMaintenanceRecordAsync([FromBody] MaintenanceRecord record, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(errors);
        }
        var vehicleExists = await _dbContext.Vehicles.AnyAsync(v => v.Id == record.VehicleId && !v.IsDeleted, cancellationToken);
        if (!vehicleExists)
            throw new BadRequestException("Vehicle not found.");

        if (record.Id == default) record.Id = Guid.NewGuid();
        record.CreatedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        await _dbContext.MaintenanceRecords.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetMaintenanceRecordAsync), new { id = record.Id }, record);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaintenanceRecordAsync(Guid id, [FromBody] MaintenanceRecord record, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(errors);
        }
        if (id != record.Id)
            throw new BadRequestException("Maintenance record ID mismatch.");

        var existingRecord = await _dbContext.MaintenanceRecords.FirstOrDefaultAsync(r => !r.IsDeleted && r.Id == id, cancellationToken);
        if (existingRecord == null)
            throw new NotFoundException("Maintenance record not found.");

        if (record.VehicleId != existingRecord.VehicleId)
        {
            var vehicleExists = await _dbContext.Vehicles.AnyAsync(v => v.Id == record.VehicleId && !v.IsDeleted, cancellationToken);
            if (!vehicleExists)
                throw new BadRequestException("Vehicle not found.");
        }

        existingRecord.VehicleId = record.VehicleId;
        existingRecord.MaintenanceType = record.MaintenanceType;
        existingRecord.Description = record.Description;
        existingRecord.Cost = record.Cost;
        existingRecord.ServiceDate = record.ServiceDate;
        existingRecord.ServiceProvider = record.ServiceProvider;
        existingRecord.NextServiceDueDate = record.NextServiceDueDate;
        existingRecord.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaintenanceRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingRecord = await _dbContext.MaintenanceRecords.FirstOrDefaultAsync(r => !r.IsDeleted && r.Id == id, cancellationToken);
        if (existingRecord == null)
            throw new NotFoundException("Maintenance record not found.");

        existingRecord.IsDeleted = true;
        existingRecord.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
