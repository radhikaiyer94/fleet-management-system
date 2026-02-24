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
public class MaintenanceRecordsController : ControllerBase
{
    private readonly MaintenanceRecordsService _maintenanceRecordsService;

    public MaintenanceRecordsController(MaintenanceRecordsService maintenanceRecordsService)
    {
        _maintenanceRecordsService = maintenanceRecordsService;
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet]
    public async Task<IActionResult> GetMaintenanceRecordsAsync([FromQuery] GetMaintenanceRecordsQuery q, CancellationToken cancellationToken = default)
    {
        var records = await _maintenanceRecordsService.GetAsync(q, cancellationToken);
        return Ok(records);
    }

    [Authorize(Roles = "Admin,FleetManager,Driver")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMaintenanceRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _maintenanceRecordsService.GetByIdAsync(id, cancellationToken);
        if (record == null)
            throw new NotFoundException("Maintenance record not found.");
        return Ok(record);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPost]
    public async Task<IActionResult> CreateMaintenanceRecordAsync([FromBody] MaintenanceRecordDTO record, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(errors);
        }
        var created = await _maintenanceRecordsService.CreateAsync(record, cancellationToken);
        return Created($"/api/maintenancerecords/{created.Id}", created);
    }

    [Authorize(Roles = "Admin,FleetManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaintenanceRecordAsync(Guid id, [FromBody] MaintenanceRecordDTO recordDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"));
            string errors = string.Join("; ", errorList.Where(m => !string.IsNullOrWhiteSpace(m)));
            throw new BadRequestException(errors);
        }
        await _maintenanceRecordsService.UpdateAsync(id, recordDto, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaintenanceRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _maintenanceRecordsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
