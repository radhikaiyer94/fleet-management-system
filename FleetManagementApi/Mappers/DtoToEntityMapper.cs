using FleetManagementApi.Domain.Entities;
using FleetManagementApi.DTO;

namespace FleetManagementApi.Mappers;

/// <summary>
/// Maps DTOs to domain entities for use in repository/database operations.
/// Only scalar and FK properties are mapped; navigation collections are left empty.
/// Caller (service) should set CreatedAt/UpdatedAt/IsDeleted as needed for create/update.
/// </summary>
public static class DtoToEntityMapper
{
    public static Vehicle? Map(VehicleDTO? dto)
    {
        if (dto == null) return null;
        return new Vehicle
        {
            Id = dto.Id == default ? Guid.NewGuid() : dto.Id,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            VIN = dto.VIN,
            LicensePlate = dto.LicensePlate,
            RegistrationDate = dto.RegistrationDate,
            PurchasePrice = dto.PurchasePrice,
            CurrentValue = dto.CurrentValue,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
            // MaintenanceRecords, Assignments: leave empty; repo uses FKs only when saving
        };
    }

    public static Driver? Map(DriverDTO? dto)
    {
        if (dto == null) return null;
        return new Driver
        {
            Id = dto.Id == default ? Guid.NewGuid() : dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            LicenseNumber = dto.LicenseNumber,
            LicenseExpiryDate = dto.LicenseExpiryDate,
            DateOfEmployment = dto.DateOfEmployment,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
            // Assignments: leave empty
        };
    }

    public static Assignment? Map(AssignmentDTO? dto)
    {
        if (dto == null) return null;
        return new Assignment
        {
            Id = dto.Id == default ? Guid.NewGuid() : dto.Id,
            VehicleId = dto.VehicleId,
            DriverId = dto.DriverId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
            // Vehicle, Driver: leave null; EF uses VehicleId/DriverId for persistence
        };
    }

    public static MaintenanceRecord? Map(MaintenanceRecordDTO? dto)
    {
        if (dto == null) return null;
        return new MaintenanceRecord
        {
            Id = dto.Id == default ? Guid.NewGuid() : dto.Id,
            VehicleId = dto.VehicleId,
            MaintenanceType = dto.MaintenanceType,
            Description = dto.Description,
            Cost = dto.Cost,
            ServiceDate = dto.ServiceDate,
            ServiceProvider = dto.ServiceProvider,
            NextServiceDueDate = dto.NextServiceDueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
            // Vehicle: leave null; EF uses VehicleId
        };
    }
}
