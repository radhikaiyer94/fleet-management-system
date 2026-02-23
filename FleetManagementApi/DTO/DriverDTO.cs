using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.DTO;

public class DriverDTO
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiryDate { get; set; }
    public DateTime DateOfEmployment { get; set; }
    public DriverStatus Status { get; set; }

    public IList<AssignmentDTO> Assignments { get; set; } = new List<AssignmentDTO>();

    public IList<VehicleDTO> Vehicles { get; set; } = new List<VehicleDTO>();
}