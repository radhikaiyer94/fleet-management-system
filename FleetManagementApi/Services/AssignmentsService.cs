using FleetManagementApi.Repositories.Interfaces;
using FleetManagementApi.DTO;
using FleetManagementApi.Mappers;
using FleetManagementApi.Models;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Domain.Enums;
using FleetManagementApi.Exceptions;

namespace FleetManagementApi.Services;

public class AssignmentsService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDriverRepository _driverRepository;

    public AssignmentsService(
        IAssignmentRepository assignmentRepository,
        IVehicleRepository vehicleRepository,
        IDriverRepository driverRepository)
    {
        _assignmentRepository = assignmentRepository;
        _vehicleRepository = vehicleRepository;
        _driverRepository = driverRepository;
    }

    public async Task<IList<AssignmentDTO>> GetAsync(GetAssignmentsQuery query, CancellationToken cancellationToken = default)
    {
        var list = await _assignmentRepository.GetAsync(query, cancellationToken);
        return EntityToDtoMapper.Map(list);
    }

    public async Task<IList<AssignmentDTO>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var list = await _assignmentRepository.GetActiveAsync(cancellationToken);
        return EntityToDtoMapper.Map(list);
    }

    public async Task<AssignmentDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, cancellationToken);
        return EntityToDtoMapper.Map(assignment);
    }

    public async Task<AssignmentDTO> CreateAsync(AssignmentDTO dto, CancellationToken cancellationToken = default)
    {
        var vehicleExists = await _vehicleRepository.GetVehicleByIdAsync(dto.VehicleId, cancellationToken);
        if (vehicleExists == null)
            throw new BadRequestException("Vehicle not found.");

        var driverExists = await _driverRepository.GetDriverByIdAsync(dto.DriverId, cancellationToken);
        if (driverExists == null)
            throw new BadRequestException("Driver not found.");

        var errors = await ValidateAssignmentAsync(dto, null, cancellationToken);
        if (errors.Count > 0)
            throw new BadRequestException(string.Join("; ", errors));

        var entity = DtoToEntityMapper.Map(dto);
        if (entity == null)
            throw new BadRequestException("Invalid assignment data.");

        var created = await _assignmentRepository.CreateAsync(entity, cancellationToken);
        return EntityToDtoMapper.Map(created)!;
    }

    public async Task UpdateAsync(Guid id, AssignmentDTO dto, CancellationToken cancellationToken = default)
    {
        if (id != dto.Id)
            throw new BadRequestException("Assignment ID mismatch.");

        var existing = await _assignmentRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
            throw new NotFoundException("Assignment not found.");

        var errors = await ValidateAssignmentAsync(dto, existing, cancellationToken);
        if (errors.Count > 0)
            throw new BadRequestException(string.Join("; ", errors));

        existing.VehicleId = dto.VehicleId;
        existing.DriverId = dto.DriverId;
        existing.StartDate = dto.StartDate;
        existing.EndDate = dto.EndDate;
        existing.Status = dto.Status;
        existing.Notes = dto.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await _assignmentRepository.UpdateAsync(existing, cancellationToken);
    }

    public async Task<AssignmentDTO> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, cancellationToken);
        if (assignment == null)
            throw new NotFoundException("Assignment not found.");

        assignment.Status = AssignmentStatus.Completed;
        assignment.EndDate = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _assignmentRepository.UpdateAsync(assignment, cancellationToken);
        return EntityToDtoMapper.Map(assignment)!;
    }

    public async Task<AssignmentDTO> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, cancellationToken);
        if (assignment == null)
            throw new NotFoundException("Assignment not found.");

        assignment.Status = AssignmentStatus.Cancelled;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _assignmentRepository.UpdateAsync(assignment, cancellationToken);
        return EntityToDtoMapper.Map(assignment)!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _assignmentRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
            throw new NotFoundException("Assignment not found.");
    }

    private async Task<List<string>> ValidateAssignmentAsync(AssignmentDTO dto, Assignment? existing, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (dto.StartDate > dto.EndDate)
            errors.Add("Start date cannot be after end date.");

        if (existing == null)
        {
            if (dto.StartDate < DateTime.UtcNow)
                errors.Add("Start date cannot be in the past.");
            if (dto.EndDate < DateTime.UtcNow)
                errors.Add("End date cannot be in the past.");
        }

        var vehicleHasActive = await _assignmentRepository.VehicleHasActiveAssignmentAsync(dto.VehicleId, existing?.Id, cancellationToken);
        if (vehicleHasActive)
            errors.Add("Vehicle already has an active assignment.");

        var driverHasActive = await _assignmentRepository.DriverHasActiveAssignmentAsync(dto.DriverId, existing?.Id, cancellationToken);
        if (driverHasActive)
            errors.Add("Driver already has an active assignment.");

        if (existing != null && dto.VehicleId != existing.VehicleId)
        {
            var vehicleExists = await _vehicleRepository.GetVehicleByIdAsync(dto.VehicleId, cancellationToken);
            if (vehicleExists == null)
                errors.Add("Vehicle not found.");
        }

        if (existing != null && dto.DriverId != existing.DriverId)
        {
            var driverExists = await _driverRepository.GetDriverByIdAsync(dto.DriverId, cancellationToken);
            if (driverExists == null)
                errors.Add("Driver not found.");
        }

        return errors;
    }
}
