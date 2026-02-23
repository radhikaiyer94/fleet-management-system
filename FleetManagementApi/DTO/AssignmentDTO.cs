using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.DTO;

public class AssignmentDTO
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AssignmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public VehicleDTO Vehicle { get; set; } = new VehicleDTO();
    public DriverDTO Driver { get; set; } = new DriverDTO();
}