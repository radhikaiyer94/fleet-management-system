using FleetManagementApi.Domain.Entities;
using FleetManagementApi.DTO;
using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.Mappers;

/// <summary>
/// Maps domain entities to their corresponding DTOs.
/// Use from the service layer after fetching entities from the repository.
/// Nested navigations are mapped shallowly (key fields only) to avoid circular references.
/// </summary>
public static class EntityToDtoMapper
{
    // -------------------------------------------------------------------------
    // Public map methods (entity -> DTO). Null entity returns null.
    // -------------------------------------------------------------------------

    public static VehicleDTO? Map(Vehicle? entity)
    {
        if (entity == null) return null;
        return new VehicleDTO
        {
            Id = entity.Id,
            Make = entity.Make,
            Model = entity.Model,
            Year = entity.Year,
            VIN = entity.VIN,
            LicensePlate = entity.LicensePlate,
            RegistrationDate = entity.RegistrationDate,
            PurchasePrice = entity.PurchasePrice,
            CurrentValue = entity.CurrentValue,
            Status = entity.Status,
            MaintenanceRecords = entity.MaintenanceRecords?.Select(Map).Where(dto => dto != null).Cast<MaintenanceRecordDTO>().ToList() ?? new List<MaintenanceRecordDTO>(),
            Assignments = entity.Assignments?.Select(a => MapAssignmentShallow(a, entity)).Where(dto => dto != null).Cast<AssignmentDTO>().ToList() ?? new List<AssignmentDTO>()
        };
    }

    public static DriverDTO? Map(Driver? entity)
    {
        if (entity == null) return null;
        return new DriverDTO
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            LicenseNumber = entity.LicenseNumber,
            LicenseExpiryDate = entity.LicenseExpiryDate,
            DateOfEmployment = entity.DateOfEmployment,
            Status = entity.Status,
            Assignments = entity.Assignments?.Select(a => MapAssignmentShallow(a, driver: entity)).Where(dto => dto != null).Cast<AssignmentDTO>().ToList() ?? new List<AssignmentDTO>(),
            Vehicles = entity.Assignments?.Select(a => a.Vehicle).Where(v => v != null).Select(v => MapVehicleMinimal(v!)!).Where(d => d != null).Cast<VehicleDTO>().DistinctBy(d => d.Id).ToList() ?? new List<VehicleDTO>()
        };
    }

    public static AssignmentDTO? Map(Assignment? entity)
    {
        if (entity == null) return null;
        return new AssignmentDTO
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            DriverId = entity.DriverId,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status,
            Notes = entity.Notes,
            Vehicle = MapVehicleMinimal(entity.Vehicle) ?? new VehicleDTO(),
            Driver = MapDriverMinimal(entity.Driver) ?? new DriverDTO()
        };
    }

    public static MaintenanceRecordDTO? Map(MaintenanceRecord? entity)
    {
        if (entity == null) return null;
        return new MaintenanceRecordDTO
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            MaintenanceType = entity.MaintenanceType,
            Description = entity.Description,
            Cost = entity.Cost,
            ServiceDate = entity.ServiceDate,
            ServiceProvider = entity.ServiceProvider,
            NextServiceDueDate = entity.NextServiceDueDate,
            Vehicle = MapVehicleMinimal(entity.Vehicle) ?? new VehicleDTO()
        };
    }

    // -------------------------------------------------------------------------
    // Collection overloads
    // -------------------------------------------------------------------------

    public static IList<VehicleDTO> Map(IEnumerable<Vehicle> entities)
        => entities.Select(Map).Where(dto => dto != null).Cast<VehicleDTO>().ToList();

    public static IList<DriverDTO> Map(IEnumerable<Driver> entities)
        => entities.Select(Map).Where(dto => dto != null).Cast<DriverDTO>().ToList();

    public static IList<AssignmentDTO> Map(IEnumerable<Assignment> entities)
        => entities.Select(Map).Where(dto => dto != null).Cast<AssignmentDTO>().ToList();

    public static IList<MaintenanceRecordDTO> Map(IEnumerable<MaintenanceRecord> entities)
        => entities.Select(Map).Where(dto => dto != null).Cast<MaintenanceRecordDTO>().ToList();

    // -------------------------------------------------------------------------
    // Shallow/minimal mappings to avoid circular reference when mapping nested
    // -------------------------------------------------------------------------

    private static VehicleDTO? MapVehicleMinimal(Vehicle? entity)
    {
        if (entity == null) return null;
        return new VehicleDTO
        {
            Id = entity.Id,
            Make = entity.Make,
            Model = entity.Model,
            Year = entity.Year,
            VIN = entity.VIN,
            LicensePlate = entity.LicensePlate,
            RegistrationDate = entity.RegistrationDate,
            PurchasePrice = entity.PurchasePrice,
            CurrentValue = entity.CurrentValue,
            Status = entity.Status
            // No MaintenanceRecords, No Assignments
        };
    }

    private static DriverDTO? MapDriverMinimal(Driver? entity)
    {
        if (entity == null) return null;
        return new DriverDTO
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            LicenseNumber = entity.LicenseNumber,
            LicenseExpiryDate = entity.LicenseExpiryDate,
            DateOfEmployment = entity.DateOfEmployment,
            Status = entity.Status
            // No Assignments, No Vehicles
        };
    }

    private static AssignmentDTO? MapAssignmentShallow(Assignment? assignment, Vehicle? vehicle = null, Driver? driver = null)
    {
        if (assignment == null) return null;
        return new AssignmentDTO
        {
            Id = assignment.Id,
            VehicleId = assignment.VehicleId,
            DriverId = assignment.DriverId,
            StartDate = assignment.StartDate,
            EndDate = assignment.EndDate,
            Status = assignment.Status,
            Notes = assignment.Notes,
            Vehicle = MapVehicleMinimal(vehicle ?? assignment.Vehicle) ?? new VehicleDTO(),
            Driver = MapDriverMinimal(driver ?? assignment.Driver) ?? new DriverDTO()
        };
    }
}
