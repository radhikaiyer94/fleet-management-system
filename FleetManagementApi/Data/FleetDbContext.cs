using Microsoft.EntityFrameworkCore;
using FleetManagementApi.Domain.Entities;

namespace FleetManagementApi.Data;

/// <summary>
/// DbContext for Fleet Management System.
/// Represents a session with the database and provides access to entity collections.
/// This context manages all database operations for vehicles, drivers, maintenance records, and assignments.
/// </summary>
public class FleetDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of FleetDbContext.
    /// Uses dependency injection to receive database configuration options.
    /// </summary>
    /// <param name="options">Database provider options (e.g., In-Memory, SQL Server, PostgreSQL)</param>
    public FleetDbContext(DbContextOptions<FleetDbContext> options) : base(options)
    {
    }

    // ============================================================================
    // DbSets - Represent database tables/collections
    // Each DbSet provides query and persistence operations for its entity type
    // ============================================================================

    /// <summary>
    /// Collection of vehicles in the fleet.
    /// Represents the Vehicles table in the database.
    /// </summary>
    public DbSet<Vehicle> Vehicles { get; set; }

    /// <summary>
    /// Collection of drivers who can operate vehicles.
    /// Represents the Drivers table in the database.
    /// </summary>
    public DbSet<Driver> Drivers { get; set; }

    /// <summary>
    /// Collection of maintenance and repair records for vehicles.
    /// Represents the MaintenanceRecords table in the database.
    /// </summary>
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }

    /// <summary>
    /// Collection of vehicle-to-driver assignments.
    /// Represents the Assignments table and creates a many-to-many relationship
    /// between Vehicles and Drivers.
    /// </summary>
    public DbSet<Assignment> Assignments { get; set; }

    /// <summary>
    /// Configures the entity models and their relationships.
    /// This method is called when the model for a derived context is being initialized.
    /// Used to configure entity properties, relationships, and database constraints.
    /// </summary>
    /// <param name="modelBuilder">Builder used to construct the model for this context</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================================
        // Vehicle Entity Configuration
        // ============================================================================
        modelBuilder.Entity<Vehicle>(entity =>
        {
            // Primary key configuration
            entity.HasKey(e => e.Id);

            // String property configurations with length constraints
            // These constraints help with data validation and database optimization
            entity.Property(e => e.Make)
                  .IsRequired()                    // Make is mandatory
                  .HasMaxLength(100);             // Limit to 100 characters (e.g., "Toyota", "Ford")

            entity.Property(e => e.Model)
                  .IsRequired()                    // Model is mandatory
                  .HasMaxLength(100);             // Limit to 100 characters (e.g., "Camry", "F-150")

            entity.Property(e => e.VIN)
                  .IsRequired()                    // VIN is mandatory (unique vehicle identifier)
                  .HasMaxLength(17);              // VIN is always 17 characters (standard format)

            entity.Property(e => e.LicensePlate)
                  .IsRequired()                    // License plate is mandatory
                  .HasMaxLength(20);               // Limit to 20 characters (varies by region)

            // Decimal property configurations with precision
            // Precision(18, 2) means: 18 total digits, 2 after decimal point
            // Example: 9999999999999999.99 (max value)
            entity.Property(e => e.PurchasePrice)
                  .HasPrecision(18, 2);           // Supports prices up to 99 trillion with 2 decimal places

            entity.Property(e => e.CurrentValue)
                  .HasPrecision(18, 2);           // Supports current market values with 2 decimal places
            
            // ========================================================================
            // Vehicle Relationships
            // ========================================================================

            // One-to-Many: One Vehicle can have many MaintenanceRecords
            // When a vehicle is deleted, all its maintenance records are also deleted (Cascade)
            entity.HasMany(e => e.MaintenanceRecords)    // Vehicle has many MaintenanceRecords
                  .WithOne(e => e.Vehicle)               // Each MaintenanceRecord belongs to one Vehicle
                  .HasForeignKey(e => e.VehicleId)        // Foreign key in MaintenanceRecord table
                  .OnDelete(DeleteBehavior.Cascade);      // Delete maintenance records when vehicle is deleted

            // One-to-Many: One Vehicle can have many Assignments
            // When a vehicle is deleted, all its assignments are also deleted (Cascade)
            entity.HasMany(e => e.Assignments)            // Vehicle has many Assignments
                  .WithOne(e => e.Vehicle)                // Each Assignment belongs to one Vehicle
                  .HasForeignKey(e => e.VehicleId)        // Foreign key in Assignment table
                  .OnDelete(DeleteBehavior.Cascade);      // Delete assignments when vehicle is deleted
        });

        // ============================================================================
        // Driver Entity Configuration
        // ============================================================================
        modelBuilder.Entity<Driver>(entity =>
        {
            // Primary key configuration
            entity.HasKey(e => e.Id);

            // String property configurations
            entity.Property(e => e.FirstName)
                  .IsRequired()                    // First name is mandatory
                  .HasMaxLength(100);             // Limit to 100 characters

            entity.Property(e => e.LastName)
                  .IsRequired()                    // Last name is mandatory
                  .HasMaxLength(100);             // Limit to 100 characters

            entity.Property(e => e.Email)
                  .HasMaxLength(255);             // Email is optional, max 255 characters (standard email length)

            entity.Property(e => e.PhoneNumber)
                  .IsRequired()                    // Phone number is mandatory
                  .HasMaxLength(20);               // Limit to 20 characters (supports international formats)

            entity.Property(e => e.LicenseNumber)
                  .IsRequired()                    // Driver's license number is mandatory
                  .HasMaxLength(50);               // Limit to 50 characters (varies by region)
            
            // ========================================================================
            // Driver Relationships
            // ========================================================================

            // One-to-Many: One Driver can have many Assignments
            // When a driver is deleted, all their assignments are also deleted (Cascade)
            entity.HasMany(e => e.Assignments)            // Driver has many Assignments
                  .WithOne(e => e.Driver)                 // Each Assignment belongs to one Driver
                  .HasForeignKey(e => e.DriverId)         // Foreign key in Assignment table
                  .OnDelete(DeleteBehavior.Cascade);      // Delete assignments when driver is deleted
        });

        // ============================================================================
        // MaintenanceRecord Entity Configuration
        // ============================================================================
        modelBuilder.Entity<MaintenanceRecord>(entity =>
        {
            // Primary key configuration
            entity.HasKey(e => e.Id);

            // String property configurations
            entity.Property(e => e.Description)
                  .HasMaxLength(500);             // Optional description, max 500 characters
                                                 // Example: "Replaced brake pads and rotors"

            // Decimal property configuration for cost
            entity.Property(e => e.Cost)
                  .HasPrecision(18, 2);          // Maintenance cost with 2 decimal places
                                                 // Example: 1250.50

            entity.Property(e => e.ServiceProvider)
                  .HasMaxLength(200);            // Optional service provider name
                                                 // Example: "ABC Auto Repair", "Dealership Service Center"
            
            // Note: Relationship to Vehicle is configured in the Vehicle entity configuration above
            // This follows EF Core best practice: configure relationships from the "one" side
        });

        // ============================================================================
        // Assignment Entity Configuration
        // ============================================================================
        // Assignment creates a many-to-many relationship between Vehicle and Driver
        // through a join table pattern (also called junction/bridge table)
        // ============================================================================
        modelBuilder.Entity<Assignment>(entity =>
        {
            // Primary key configuration
            entity.HasKey(e => e.Id);

            // String property configuration
            entity.Property(e => e.Notes)
                  .HasMaxLength(500);            // Optional notes about the assignment
                                                 // Example: "Temporary assignment for delivery route"
            
            // Note: Relationships to Vehicle and Driver are configured in their respective entities above
            // This ensures proper foreign key constraints and cascade delete behavior
            // 
            // Assignment Structure:
            // - VehicleId (FK) -> Links to Vehicle
            // - DriverId (FK) -> Links to Driver
            // - StartDate, EndDate -> Tracks assignment period
            // - Status -> Tracks assignment state (Active, Completed, Cancelled)
        });
    }
}
