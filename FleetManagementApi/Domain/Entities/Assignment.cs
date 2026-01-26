using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.Domain.Entities
{
    public class Assignment
    {
        public int Id { get; set; }
        public required int VehicleId { get; set; }
        public required int DriverId { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public AssignmentStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // ============================================================================
        // Navigation Properties - Entity Framework Core Relationships
        // Assignment creates a many-to-many relationship between Vehicle and Driver
        // through a join table pattern (also called junction/bridge table)
        // ============================================================================

        /// <summary>
        /// The vehicle assigned in this assignment.
        /// Represents a many-to-one relationship: Many Assignments belong to one Vehicle.
        /// Use this to access the vehicle: assignment.Vehicle
        /// The VehicleId property is the foreign key that links to Vehicle.Id
        /// </summary>
        public Vehicle Vehicle { get; set; } = null!;

        /// <summary>
        /// The driver assigned in this assignment.
        /// Represents a many-to-one relationship: Many Assignments belong to one Driver.
        /// Use this to access the driver: assignment.Driver
        /// The DriverId property is the foreign key that links to Driver.Id
        /// </summary>
        public Driver Driver { get; set; } = null!;
    }
}