using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Entities;

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
    public IActionResult GetVehicles()
    {
        var vehicles = _dbContext.Vehicles.Where(vehicle => !vehicle.IsDeleted).ToList();
        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public IActionResult GetVehicle(Guid id)
    {
        var vehicle = _dbContext.Vehicles.Where(v => !v.IsDeleted).FirstOrDefault(v => v.Id == id);
        return vehicle != null ? Ok(vehicle) : NotFound(new { message = "Vehicle not found."});
    }

    [HttpGet("{id}/maintenance-records")]
    public IActionResult GetVehicleMaintenanceRecords(Guid id)
    {
        var existingVehicle = _dbContext.Vehicles.Where(v => !v.IsDeleted).FirstOrDefault(v => v.Id == id);
        if (existingVehicle == null)
        {
            return NotFound(new { message = "Vehicle not found."});
        }

        var maintenanceRecords = _dbContext.MaintenanceRecords.Where(record => record.VehicleId == id).ToList();
        return maintenanceRecords.Count > 0 ? Ok(maintenanceRecords) : NotFound(new { message = "No maintenance records found for this vehicle."});
    }

    [HttpGet("{id}/assignments")]
    public IActionResult GetVehicleAssignments(Guid id)
    {
        var existingVehicle = _dbContext.Vehicles.Where(v => !v.IsDeleted).FirstOrDefault(v => v.Id == id);
        if (existingVehicle == null)
        {   
            return NotFound(new { message = "Vehicle not found."});
        }
        var assignments = _dbContext.Assignments.Where(a => a.VehicleId == id).ToList();
        return assignments.Count > 0 ? Ok(assignments) : NotFound(new { message = "No assignments found for this vehicle."});

    }

    [HttpPost]
    public IActionResult CreateVehicle([FromBody] Vehicle vehicle)
    {
        if (vehicle.Id == default) vehicle.Id = Guid.NewGuid();
        vehicle.CreatedAt = DateTime.UtcNow;
        vehicle.UpdatedAt = DateTime.UtcNow;

        _dbContext.Vehicles.Add(vehicle);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateVehicle(Guid id, [FromBody] Vehicle vehicle)
    {
        if (id != vehicle.Id)
        {
            return BadRequest(new { message = "Vehicle ID mismatch."});
        }
        var existingVehicle = _dbContext.Vehicles.Where(v => !v.IsDeleted).FirstOrDefault(v => v.Id == id);
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

        _dbContext.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteVehicle(Guid id)
    {
        var existingVehicle = _dbContext.Vehicles.Where(v => !v.IsDeleted).FirstOrDefault(v => v.Id == id);
        if (existingVehicle == null)
        {
            return NotFound(new { message = "Vehicle not found."});
        }
        existingVehicle.IsDeleted = true;
        existingVehicle.UpdatedAt = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return NoContent();
    }
}