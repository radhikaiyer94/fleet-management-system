using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.Domain.Entities
{
    public class Driver
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string LicenseNumber { get; set; }
        public required DateTime LicenseExpiryDate { get; set; }
        public required DateTime DateOfEmployment { get; set; }
        public DriverStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // ============================================================================
        // Navigation Properties - Entity Framework Core Relationships
        // ============================================================================

        /// <summary>
        /// Collection of all vehicle assignments for this driver.
        /// Represents a one-to-many relationship: One Driver can have many Assignments.
        /// Use this to access assignment history: driver.Assignments
        /// Note: Assignment creates a many-to-many relationship between Driver and Vehicle
        /// </summary>
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}