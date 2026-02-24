using FleetManagementApi.Repositories.Interfaces;
using FleetManagementApi.DTO;
using FleetManagementApi.Mappers;
using FleetManagementApi.Models;
using FleetManagementApi.Exceptions;

namespace FleetManagementApi.Services;

public class MaintenanceRecordsService
{
    private readonly IMaintenanceRecordRepository _recordRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public MaintenanceRecordsService(IMaintenanceRecordRepository recordRepository, IVehicleRepository vehicleRepository)
    {
        _recordRepository = recordRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IList<MaintenanceRecordDTO>> GetAsync(GetMaintenanceRecordsQuery query, CancellationToken cancellationToken = default)
    {
        var records = await _recordRepository.GetAsync(query, cancellationToken);
        return EntityToDtoMapper.Map(records);
    }

    public async Task<MaintenanceRecordDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _recordRepository.GetByIdAsync(id, cancellationToken);
        return EntityToDtoMapper.Map(record);
    }

    public async Task<MaintenanceRecordDTO> CreateAsync(MaintenanceRecordDTO dto, CancellationToken cancellationToken = default)
    {
        var vehicleExists = await _vehicleRepository.GetVehicleByIdAsync(dto.VehicleId, cancellationToken);
        if (vehicleExists == null)
            throw new BadRequestException("Vehicle not found.");

        var entity = DtoToEntityMapper.Map(dto);
        if (entity == null)
            throw new BadRequestException("Invalid maintenance record data.");

        var created = await _recordRepository.CreateAsync(entity, cancellationToken);
        return EntityToDtoMapper.Map(created)!;
    }

    public async Task UpdateAsync(Guid id, MaintenanceRecordDTO dto, CancellationToken cancellationToken = default)
    {
        if (id != dto.Id)
            throw new BadRequestException("Maintenance record ID mismatch.");

        var existing = await _recordRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
            throw new NotFoundException("Maintenance record not found.");

        if (dto.VehicleId != existing.VehicleId)
        {
            var vehicleExists = await _vehicleRepository.GetVehicleByIdAsync(dto.VehicleId, cancellationToken);
            if (vehicleExists == null)
                throw new BadRequestException("Vehicle not found.");
        }

        existing.VehicleId = dto.VehicleId;
        existing.MaintenanceType = dto.MaintenanceType;
        existing.Description = dto.Description;
        existing.Cost = dto.Cost;
        existing.ServiceDate = dto.ServiceDate;
        existing.ServiceProvider = dto.ServiceProvider;
        existing.NextServiceDueDate = dto.NextServiceDueDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await _recordRepository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _recordRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
            throw new NotFoundException("Maintenance record not found.");
    }
}
