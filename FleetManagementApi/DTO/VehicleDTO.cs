using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.DTO;

public class VehicleDTO
{
    public Guid Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string VIN { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal CurrentValue { get; set; }
    public VehicleStatus Status { get; set; }

    public IList<MaintenanceRecordDTO> MaintenanceRecords { get; set; } = new List<MaintenanceRecordDTO>();

    public IList<AssignmentDTO> Assignments { get; set; } = new List<AssignmentDTO>();
}