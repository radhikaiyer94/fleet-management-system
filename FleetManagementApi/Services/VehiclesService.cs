using FleetManagementApi.Repositories.Interfaces;
using FleetManagementApi.DTO;
using FleetManagementApi.Mappers;
using FleetManagementApi.Models;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Exceptions;

namespace FleetManagementApi.Services;

public class VehiclesService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehiclesService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IList<VehicleDTO>> GetVehiclesAsync(GetVehiclesQuery query, CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetVehiclesAsync(query, cancellationToken);
        return EntityToDtoMapper.Map(vehicles);
    }

    public async Task<VehicleDTO?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetVehicleByIdAsync(id, cancellationToken);
        return EntityToDtoMapper.Map(vehicle);
    }

    public async Task<VehicleDTO> CreateVehicleAsync(VehicleDTO vehicleDto, CancellationToken cancellationToken = default)
    {
        var vehicleEntity = DtoToEntityMapper.Map(vehicleDto);
        if(vehicleEntity == null)
            throw new BadRequestException("Invalid vehicle data.");

        var createdVehicle = await _vehicleRepository.CreateVehicleAsync(vehicleEntity, cancellationToken);
        return EntityToDtoMapper.Map(createdVehicle)!;
    }

    public async Task UpdateVehicleAsync(Guid id, VehicleDTO vehicleDto, CancellationToken cancellationToken = default)
    {
        if (id != vehicleDto.Id)
            throw new BadRequestException("Vehicle ID mismatch.");

        var existingVehicle = await _vehicleRepository.GetVehicleByIdAsync(id, cancellationToken);
        if (existingVehicle == null)
            throw new NotFoundException("Vehicle not found.");

        if (VehicleDataEquals(existingVehicle, vehicleDto))
            return;

        existingVehicle.Make = vehicleDto.Make;
        existingVehicle.Model = vehicleDto.Model;
        existingVehicle.Year = vehicleDto.Year;
        existingVehicle.VIN = vehicleDto.VIN;
        existingVehicle.LicensePlate = vehicleDto.LicensePlate;
        existingVehicle.RegistrationDate = vehicleDto.RegistrationDate;
        existingVehicle.PurchasePrice = vehicleDto.PurchasePrice;
        existingVehicle.CurrentValue = vehicleDto.CurrentValue;
        existingVehicle.Status = vehicleDto.Status;
        existingVehicle.UpdatedAt = DateTime.UtcNow;

        await _vehicleRepository.UpdateVehicleAsync(existingVehicle, cancellationToken);
    }

    public async Task DeleteVehicleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _vehicleRepository.DeleteVehicleAsync(id, cancellationToken);
        if (!deleted)
            throw new NotFoundException("Vehicle not found.");
    }

    public async Task<IList<MaintenanceRecordDTO>> GetVehicleMaintenanceRecordsAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var existing = await _vehicleRepository.GetVehicleByIdAsync(vehicleId, cancellationToken);
        if (existing == null)
            throw new NotFoundException("Vehicle not found.");

        var records = await _vehicleRepository.GetMaintenanceRecordsByVehicleIdAsync(vehicleId, cancellationToken);
        return EntityToDtoMapper.Map(records);
    }

    public async Task<IList<AssignmentDTO>> GetVehicleAssignmentsAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var existing = await _vehicleRepository.GetVehicleByIdAsync(vehicleId, cancellationToken);
        if (existing == null)
            throw new NotFoundException("Vehicle not found.");

        var assignments = await _vehicleRepository.GetAssignmentsByVehicleIdAsync(vehicleId, cancellationToken);
        return EntityToDtoMapper.Map(assignments);
    }

    private static bool VehicleDataEquals(Vehicle existing, VehicleDTO incoming) =>
        existing.Make == incoming.Make &&
        existing.Model == incoming.Model &&
        existing.Year == incoming.Year &&
        existing.VIN == incoming.VIN &&
        existing.LicensePlate == incoming.LicensePlate &&
        existing.RegistrationDate == incoming.RegistrationDate &&
        existing.PurchasePrice == incoming.PurchasePrice &&
        existing.CurrentValue == incoming.CurrentValue &&
        existing.Status == incoming.Status;
}
