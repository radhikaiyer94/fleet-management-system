using FleetManagementApi.Repositories.Interfaces;
using FleetManagementApi.DTO;
using FleetManagementApi.Mappers;
using FleetManagementApi.Models;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Exceptions;

namespace FleetManagementApi.Services;

public class DriversService
{
    private readonly IDriverRepository _driverRepository;

    public DriversService(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task<IList<DriverDTO>> GetDriversAsync(GetDriversQuery query, CancellationToken cancellationToken = default)
    {
        var drivers = await _driverRepository.GetDriversAsync(query, cancellationToken);
        return EntityToDtoMapper.Map(drivers);
    }

    public async Task<DriverDTO?> GetDriverByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _driverRepository.GetDriverByIdAsync(id, cancellationToken);
        return EntityToDtoMapper.Map(driver);
    }

    public async Task<IList<AssignmentDTO>> GetDriverAssignmentsAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var existing = await _driverRepository.GetDriverByIdAsync(driverId, cancellationToken);
        if (existing == null)
            throw new NotFoundException("Driver not found.");

        var assignments = await _driverRepository.GetAssignmentsByDriverIdAsync(driverId, cancellationToken);
        return EntityToDtoMapper.Map(assignments);
    }

    public async Task<DriverDTO> CreateDriverAsync(DriverDTO driverDto, CancellationToken cancellationToken = default)
    {
        var entity = DtoToEntityMapper.Map(driverDto);
        if (entity == null)
            throw new BadRequestException("Invalid driver data.");

        var created = await _driverRepository.CreateDriverAsync(entity, cancellationToken);
        return EntityToDtoMapper.Map(created)!;
    }

    public async Task UpdateDriverAsync(Guid id, DriverDTO driverDto, CancellationToken cancellationToken = default)
    {
        if (id != driverDto.Id)
            throw new BadRequestException("Driver ID mismatch.");

        var existing = await _driverRepository.GetDriverByIdAsync(id, cancellationToken);
        if (existing == null)
            throw new NotFoundException("Driver not found.");

        if (DriverDataEquals(existing, driverDto))
            return;

        existing.FirstName = driverDto.FirstName;
        existing.LastName = driverDto.LastName;
        existing.Email = driverDto.Email;
        existing.PhoneNumber = driverDto.PhoneNumber;
        existing.LicenseNumber = driverDto.LicenseNumber;
        existing.LicenseExpiryDate = driverDto.LicenseExpiryDate;
        existing.DateOfEmployment = driverDto.DateOfEmployment;
        existing.Status = driverDto.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await _driverRepository.UpdateDriverAsync(existing, cancellationToken);
    }

    public async Task DeleteDriverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _driverRepository.DeleteDriverAsync(id, cancellationToken);
        if (!deleted)
            throw new NotFoundException("Driver not found.");
    }

    private static bool DriverDataEquals(Driver existing, DriverDTO incoming) =>
        existing.FirstName == incoming.FirstName &&
        existing.LastName == incoming.LastName &&
        existing.Email == incoming.Email &&
        existing.PhoneNumber == incoming.PhoneNumber &&
        existing.LicenseNumber == incoming.LicenseNumber &&
        existing.LicenseExpiryDate == incoming.LicenseExpiryDate &&
        existing.DateOfEmployment == incoming.DateOfEmployment &&
        existing.Status == incoming.Status;
}
