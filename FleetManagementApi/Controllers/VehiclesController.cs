using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Data;
using FleetManagementApi.Exceptions;
using FleetManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using FleetManagementApi.Services;
using FleetManagementApi.DTO;

namespace FleetManagementApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly VehiclesService _vehiclesService;
    public VehiclesController(VehiclesService vehiclesService)
    {
        _vehiclesService = vehiclesService;
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet]
    public async Task<IActionResult> GetVehiclesAsync([FromQuery] GetVehiclesQuery q, CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehiclesService.GetVehiclesAsync(q, cancellationToken);
        return Ok(vehicles);
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehiclesService.GetVehicleByIdAsync(id, cancellationToken);
        if (vehicle == null)
            throw new NotFoundException("Vehicle not found.");

        return Ok(vehicle);
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet("{id}/maintenance-records")]
    public async Task<IActionResult> GetVehicleMaintenanceRecordsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var maintenanceRecords = await _vehiclesService.GetVehicleMaintenanceRecordsAsync(id, cancellationToken);
        if (maintenanceRecords.Count == 0)
            throw new NotFoundException("No maintenance records found for this vehicle.");

        return Ok(maintenanceRecords);
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet("{id}/assignments")]
    public async Task<IActionResult> GetVehicleAssignmentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignments = await _vehiclesService.GetVehicleAssignmentsAsync(id, cancellationToken);
        if (assignments.Count == 0)
            throw new NotFoundException("No assignments found for this vehicle.");

        return Ok(assignments);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPost]
    public async Task<IActionResult> CreateVehicleAsync([FromBody] VehicleDTO vehicle, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(errors);
        }
        var createdVehicle = await _vehiclesService.CreateVehicleAsync(vehicle, cancellationToken);
        return Created($"/api/vehicles/{createdVehicle.Id}", createdVehicle);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicleAsync(Guid id, [FromBody] VehicleDTO vehicleDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            Console.WriteLine(errors);
            throw new BadRequestException(errors);
        }

        await _vehiclesService.UpdateVehicleAsync(id, vehicleDto, cancellationToken);

        return NoContent();
    }


    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _vehiclesService.DeleteVehicleAsync(id, cancellationToken);

        return Ok(new { message = "Vehicle deleted successfully" });
    }
}