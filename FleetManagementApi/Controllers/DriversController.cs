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
public class DriversController : ControllerBase
{
    private readonly DriversService _driversService;

    public DriversController(DriversService driversService)
    {
        _driversService = driversService;
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet]
    public async Task<IActionResult> GetDriversAsync([FromQuery] GetDriversQuery q, CancellationToken cancellationToken = default)
    {
        var drivers = await _driversService.GetDriversAsync(q, cancellationToken);
        return Ok(drivers);
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDriverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _driversService.GetDriverByIdAsync(id, cancellationToken);
        if (driver == null)
            throw new NotFoundException("Driver not found.");
        return Ok(driver);
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet("{id}/assignments")]
    public async Task<IActionResult> GetDriverAssignmentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignments = await _driversService.GetDriverAssignmentsAsync(id, cancellationToken);
        if (assignments.Count == 0)
            throw new NotFoundException("No assignments found for this driver.");
        return Ok(assignments);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPost]
    public async Task<IActionResult> CreateDriverAsync([FromBody] DriverDTO driver, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(errors);
        }
        var created = await _driversService.CreateDriverAsync(driver, cancellationToken);
        return Created($"/api/drivers/{created.Id}", created);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDriverAsync(Guid id, [FromBody] DriverDTO driverDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(errors);
        }
        await _driversService.UpdateDriverAsync(id, driverDto, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDriverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _driversService.DeleteDriverAsync(id, cancellationToken);
        return NoContent();
    }
}
