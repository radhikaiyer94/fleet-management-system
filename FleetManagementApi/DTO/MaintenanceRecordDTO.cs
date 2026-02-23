using FleetManagementApi.Domain.Enums;
namespace FleetManagementApi.DTO;

public class MaintenanceRecordDTO
{
    public Guid Id { get; set; }

    public required Guid VehicleId { get; set; }

    public MaintenanceType MaintenanceType { get; set; }

    public string? Description { get; set; }

    public required decimal Cost { get; set; }

    public required DateTime ServiceDate { get; set; }

    public string? ServiceProvider { get; set; }

    public DateTime NextServiceDueDate { get; set; }

    public VehicleDTO Vehicle { get; set; } = new VehicleDTO();
}