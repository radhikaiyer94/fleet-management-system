using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.Domain.Entities
{
    public class MaintenanceRecord
    {
        public Guid Id { get; set; }

        public required Guid VehicleId { get; set; }

        public MaintenanceType MaintenanceType { get; set; }

        public string? Description { get; set; }

        public required decimal Cost { get; set; }

        public required DateTime ServiceDate { get; set; }

        public string? ServiceProvider { get; set; }

        public DateTime NextServiceDueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        // ============================================================================
        // Navigation Properties - Entity Framework Core Relationships
        // ============================================================================

        /// <summary>
        /// The vehicle this maintenance record belongs to.
        /// Represents a many-to-one relationship: Many MaintenanceRecords belong to one Vehicle.
        /// Use this to access the vehicle: maintenanceRecord.Vehicle
        /// The VehicleId property is the foreign key that links to Vehicle.Id
        /// </summary>
        public Vehicle Vehicle { get; set; } = null!;
    }
}