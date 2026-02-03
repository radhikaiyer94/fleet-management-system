using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.Domain.Entities
{
    public class Vehicle
    {
        public Guid Id { get; set; }
        public required string Make { get; set; }

        public required string Model { get; set; }

        public int Year { get; set; }

        public required string VIN { get; set; }

        public required string LicensePlate { get; set; }

        public DateTime RegistrationDate { get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal CurrentValue { get; set; }

        public VehicleStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        // ============================================================================
        // Navigation Properties - Entity Framework Core Relationships
        // These properties enable accessing related entities without writing explicit joins
        // ============================================================================

        /// <summary>
        /// Collection of all maintenance records for this vehicle.
        /// Represents a one-to-many relationship: One Vehicle can have many MaintenanceRecords.
        /// Use this to access maintenance history: vehicle.MaintenanceRecords
        /// </summary>
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();

        /// <summary>
        /// Collection of all assignments (driver assignments) for this vehicle.
        /// Represents a one-to-many relationship: One Vehicle can have many Assignments.
        /// Use this to access assignment history: vehicle.Assignments
        /// </summary>
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}