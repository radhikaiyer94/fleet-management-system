namespace FleetManagementApi.Domain.Enums;

public enum VehicleStatus
{
    Available,

    NotAvailable,

    UnderMaintenance
}

public enum DriverStatus
{
    Active,

    Inactive,

    Suspended
}

public enum MaintenanceType
{
    Cleaning,

    Washing,

    OilChange,

    TireReplacement,

    BrakeService,

    Inspection,

    Repair,

    Other
}

public enum AssignmentStatus
{
    Active,

    Completed,

    Cancelled
}

public enum UserRole
{
    Admin,
    FleetManager,
    Driver
}