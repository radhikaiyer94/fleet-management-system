using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Domain.Enums;
using FleetManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly FleetDbContext _dbContext;
    public VehiclesController(FleetDbContext context)
    {
        _dbContext = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetVehiclesAsync([FromQuery] GetVehiclesQuery q, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Vehicles.Where(v => !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(v =>
                (v.Make != null && v.Make.ToLower().Contains(term)) ||
                (v.Model != null && v.Model.ToLower().Contains(term)) ||
                (v.VIN != null && v.VIN.ToLower().Contains(term)) ||
                (v.LicensePlate != null && v.LicensePlate.ToLower().Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(q.Make))
        {
            var makeTerm = q.Make.Trim().ToLower();
            query = query.Where(v => v.Make != null && v.Make.ToLower().Contains(makeTerm));
        }
        if (!string.IsNullOrWhiteSpace(q.Model))
        {
            var modelTerm = q.Model.Trim().ToLower();
            query = query.Where(v => v.Model != null && v.Model.ToLower().Contains(modelTerm));
        }
        if (q.Year.HasValue)
            query = query.Where(v => v.Year == q.Year.Value);

        if (q.Status.HasValue)
            query = query.Where(v => v.Status == q.Status.Value);

        if (!string.IsNullOrWhiteSpace(q.Vin))
        {
            var vinTerm = q.Vin.Trim().ToLower();
            query = query.Where(v => v.VIN != null && v.VIN.ToLower().Contains(vinTerm));
        }
        if (!string.IsNullOrWhiteSpace(q.LicensePlate))
        {
            var plateTerm = q.LicensePlate.Trim().ToLower();
            query = query.Where(v => v.LicensePlate != null && v.LicensePlate.ToLower().Contains(plateTerm));
        }

        // Sort: use explicit properties so EF Core can translate to SQL (reflection cannot)
        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            var sortByLower = q.SortBy.Trim().ToLower();
            var desc = string.Equals(q.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            query = sortByLower switch
            {
                "id" => desc ? query.OrderByDescending(v => v.Id) : query.OrderBy(v => v.Id),
                "make" => desc ? query.OrderByDescending(v => v.Make) : query.OrderBy(v => v.Make),
                "model" => desc ? query.OrderByDescending(v => v.Model) : query.OrderBy(v => v.Model),
                "year" => desc ? query.OrderByDescending(v => v.Year) : query.OrderBy(v => v.Year),
                "status" => desc ? query.OrderByDescending(v => v.Status) : query.OrderBy(v => v.Status),
                "createdat" => desc ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt),
                "vin" => desc ? query.OrderByDescending(v => v.VIN) : query.OrderBy(v => v.VIN),
                "licenseplate" => desc ? query.OrderByDescending(v => v.LicensePlate) : query.OrderBy(v => v.LicensePlate),
                _ => query.OrderBy(v => v.Id)
            };
        }
        else
        {
            query = query.OrderBy(v => v.Id);
        }

        if (q.Page > 0 && q.PageSize > 0)
                query = query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize);

        var vehicles = await query.ToListAsync(cancellationToken);
        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => !v.IsDeleted && v.Id == id, cancellationToken);
        return vehicle != null ? Ok(vehicle) : NotFound(new { message = "Vehicle not found."});
    }

    [HttpGet("{id}/maintenance-records")]
    public async Task<IActionResult> GetVehicleMaintenanceRecordsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingVehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => !v.IsDeleted && v.Id == id, cancellationToken);
        if (existingVehicle == null)
        {
            return NotFound(new { message = "Vehicle not found."});
        }

        var maintenanceRecords = await _dbContext.MaintenanceRecords.Where(r => !r.IsDeleted && r.VehicleId == id).ToListAsync(cancellationToken);
        return maintenanceRecords.Count > 0 ? Ok(maintenanceRecords) : NotFound(new { message = "No maintenance records found for this vehicle."});
    }

    [HttpGet("{id}/assignments")]
    public async Task<IActionResult> GetVehicleAssignmentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingVehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v =>!v.IsDeleted &&  v.Id == id, cancellationToken);
        if (existingVehicle == null)
        {   
            return NotFound(new { message = "Vehicle not found."});
        }
        var assignments = await _dbContext.Assignments.Where(a => !a.IsDeleted && a.VehicleId == id).ToListAsync(cancellationToken);
        return assignments.Count > 0 ? Ok(assignments) : NotFound(new { message = "No assignments found for this vehicle."});

    }

    [HttpPost]
    public async Task<IActionResult> CreateVehicleAsync([FromBody] Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        if (vehicle.Id == default) vehicle.Id = Guid.NewGuid();
        vehicle.CreatedAt = DateTime.UtcNow;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetVehicleAsync), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicleAsync(Guid id, [FromBody] Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        if (id != vehicle.Id)
        {
            return BadRequest(new { message = "Vehicle ID mismatch."});
        }
        var existingVehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => !v.IsDeleted && v.Id == id, cancellationToken);
        if (existingVehicle == null)
        {
            return NotFound(new { message = "Vehicle not found."});
        }
        existingVehicle.Make = vehicle.Make;
        existingVehicle.Model = vehicle.Model;
        existingVehicle.Year = vehicle.Year;
        existingVehicle.VIN = vehicle.VIN;
        existingVehicle.LicensePlate = vehicle.LicensePlate;
        existingVehicle.RegistrationDate = vehicle.RegistrationDate;
        existingVehicle.PurchasePrice = vehicle.PurchasePrice;
        existingVehicle.CurrentValue = vehicle.CurrentValue;
        existingVehicle.Status = vehicle.Status;
        existingVehicle.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingVehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => !v.IsDeleted && v.Id == id, cancellationToken);
        if (existingVehicle == null)
        {
            return NotFound(new { message = "Vehicle not found."});
        }
        existingVehicle.IsDeleted = true;
        existingVehicle.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}