using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Entities;

namespace FleetManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceRecordsController : ControllerBase
{
    private readonly FleetDbContext _dbContext;

    public MaintenanceRecordsController(FleetDbContext context)
    {
        _dbContext = context;
    }

    [HttpGet]
    public IActionResult GetMaintenanceRecords()
    {
        var records = _dbContext.MaintenanceRecords.Where(record => !record.IsDeleted).ToList();
        return Ok(records);
    }

    [HttpGet("{id}")]
    public IActionResult GetMaintenanceRecord(Guid id)
    {
        var record = _dbContext.MaintenanceRecords.Where(r => !r.IsDeleted).FirstOrDefault(r => r.Id == id);
        return record != null ? Ok(record) : NotFound(new { message = "Maintenance record not found." });
    }

    [HttpPost]
    public IActionResult CreateMaintenanceRecord([FromBody] MaintenanceRecord record)
    {
        // Validate that the vehicle exists
        var vehicleExists = _dbContext.Vehicles.Any(v => v.Id == record.VehicleId && !v.IsDeleted);
        if (!vehicleExists)
        {
            return BadRequest(new { message = "Vehicle not found." });
        }

        if (record.Id == default) record.Id = Guid.NewGuid();
        record.CreatedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        _dbContext.MaintenanceRecords.Add(record);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetMaintenanceRecord), new { id = record.Id }, record);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateMaintenanceRecord(Guid id, [FromBody] MaintenanceRecord record)
    {
        if (id != record.Id)
        {
            return BadRequest(new { message = "Maintenance record ID mismatch." });
        }

        var existingRecord = _dbContext.MaintenanceRecords.Where(r => !r.IsDeleted).FirstOrDefault(r => r.Id == id);
        if (existingRecord == null)
        {
            return NotFound(new { message = "Maintenance record not found." });
        }

        // Validate vehicle if VehicleId is changing
        if (record.VehicleId != existingRecord.VehicleId)
        {
            var vehicleExists = _dbContext.Vehicles.Any(v => v.Id == record.VehicleId && !v.IsDeleted);
            if (!vehicleExists)
            {
                return BadRequest(new { message = "Vehicle not found." });
            }
        }

        existingRecord.VehicleId = record.VehicleId;
        existingRecord.MaintenanceType = record.MaintenanceType;
        existingRecord.Description = record.Description;
        existingRecord.Cost = record.Cost;
        existingRecord.ServiceDate = record.ServiceDate;
        existingRecord.ServiceProvider = record.ServiceProvider;
        existingRecord.NextServiceDueDate = record.NextServiceDueDate;
        existingRecord.UpdatedAt = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteMaintenanceRecord(Guid id)
    {
        var existingRecord = _dbContext.MaintenanceRecords.Where(r => !r.IsDeleted).FirstOrDefault(r => r.Id == id);
        if (existingRecord == null)
        {
            return NotFound(new { message = "Maintenance record not found." });
        }

        existingRecord.IsDeleted = true;
        existingRecord.UpdatedAt = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return NoContent();
    }
}
