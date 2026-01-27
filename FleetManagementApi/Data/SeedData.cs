using FleetManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.Data;

/// <summary>
/// Provides seed data for initializing the database with sample records.
/// This class contains static methods to populate the database with realistic test data.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Seeds the database with initial sample data.
    /// This method is idempotent - it checks if data exists before adding to avoid duplicates.
    /// Call this in Program.cs during application startup.
    /// </summary>
    /// <param name="context">The FleetDbContext instance</param>
    public static void Initialize(FleetDbContext context)
    {
        // Only seed if database doesn't already have data
        if (context.Vehicles.Any() || context.Drivers.Any())
        {
            return;
        }

        // ========================================================================
        // Seed Vehicles
        // ========================================================================
        var vehicles = new List<Vehicle>
        {
            new Vehicle
            {
                Make = "Toyota",
                Model = "Camry",
                Year = 2022,
                VIN = "4T1BF1AK8CU123456",
                LicensePlate = "ABC1234",
                RegistrationDate = new DateTime(2022, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                PurchasePrice = 28000.00m,
                CurrentValue = 24500.00m,
                Status = VehicleStatus.Available,
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                Make = "Ford",
                Model = "F-150",
                Year = 2021,
                VIN = "1FTFW1ET5DFC10897",
                LicensePlate = "XYZ9876",
                RegistrationDate = new DateTime(2022, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                PurchasePrice = 45000.00m,
                CurrentValue = 38000.00m,
                Status = VehicleStatus.Available,
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                Make = "Honda",
                Model = "Civic",
                Year = 2023,
                VIN = "2HGCV52353H123456",
                LicensePlate = "HND2023",
                RegistrationDate = new DateTime(2023, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                PurchasePrice = 26500.00m,
                CurrentValue = 25000.00m,
                Status = VehicleStatus.Available,
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                Make = "Chevrolet",
                Model = "Silverado",
                Year = 2020,
                VIN = "3GCUKREH3LG531579",
                LicensePlate = "CHV2020",
                RegistrationDate = new DateTime(2021, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                PurchasePrice = 50000.00m,
                CurrentValue = 42000.00m,
                Status = VehicleStatus.UnderMaintenance,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Vehicles.AddRange(vehicles);
        context.SaveChanges();

        // ========================================================================
        // Seed Drivers
        // ========================================================================
        var drivers = new List<Driver>
        {
            new Driver
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@fleet.com",
                PhoneNumber = "+1-555-0101",
                LicenseNumber = "D1234567",
                LicenseExpiryDate = DateTime.UtcNow.AddYears(2),
                DateOfEmployment = DateTime.UtcNow.AddYears(-1),
                Status = DriverStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Driver
            {
                FirstName = "Sarah",
                LastName = "Johnson",
                Email = "sarah.johnson@fleet.com",
                PhoneNumber = "+1-555-0102",
                LicenseNumber = "D2345678",
                LicenseExpiryDate = DateTime.UtcNow.AddYears(3),
                DateOfEmployment = DateTime.UtcNow.AddYears(-2),
                Status = DriverStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Driver
            {
                FirstName = "Michael",
                LastName = "Brown",
                Email = "michael.brown@fleet.com",
                PhoneNumber = "+1-555-0103",
                LicenseNumber = "D3456789",
                LicenseExpiryDate = DateTime.UtcNow.AddYears(1),
                DateOfEmployment = DateTime.UtcNow.AddYears(-3),
                Status = DriverStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Driver
            {
                FirstName = "Emily",
                LastName = "Davis",
                Email = "emily.davis@fleet.com",
                PhoneNumber = "+1-555-0104",
                LicenseNumber = "D4567890",
                LicenseExpiryDate = DateTime.UtcNow.AddMonths(6),
                DateOfEmployment = DateTime.UtcNow.AddYears(-4),
                Status = DriverStatus.Inactive,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Drivers.AddRange(drivers);
        context.SaveChanges();

        // ========================================================================
        // Seed Maintenance Records
        // ========================================================================
        var maintenanceRecords = new List<MaintenanceRecord>
        {
            new MaintenanceRecord
            {
                VehicleId = vehicles[0].Id,
                MaintenanceType = MaintenanceType.OilChange,
                Description = "Regular oil change and filter replacement",
                ServiceDate = DateTime.UtcNow.AddDays(-30),
                Cost = 75.00m,
                ServiceProvider = "ABC Auto Service",
                CreatedAt = DateTime.UtcNow
            },
            new MaintenanceRecord
            {
                VehicleId = vehicles[1].Id,
                MaintenanceType = MaintenanceType.TireReplacement,
                Description = "Replaced two tires and checked alignment",
                ServiceDate = DateTime.UtcNow.AddDays(-15),
                Cost = 120.00m,
                ServiceProvider = "XYZ Tire Center",
                CreatedAt = DateTime.UtcNow
            },
            new MaintenanceRecord
            {
                VehicleId = vehicles[3].Id,
                MaintenanceType = MaintenanceType.BrakeService,
                Description = "Replaced brake pads and rotors",
                ServiceDate = DateTime.UtcNow.AddDays(-5),
                Cost = 450.00m,
                ServiceProvider = "Premium Auto Care",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.MaintenanceRecords.AddRange(maintenanceRecords);
        context.SaveChanges();

        // ========================================================================
        // Seed Assignments (Vehicle-Driver assignments)
        // ========================================================================
        var assignments = new List<Assignment>
        {
            new Assignment
            {
                VehicleId = vehicles[0].Id,
                DriverId = drivers[0].Id,
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = AssignmentStatus.Active,
                Notes = "Primary delivery vehicle for John Smith",
                CreatedAt = DateTime.UtcNow
            },
            new Assignment
            {
                VehicleId = vehicles[1].Id,
                DriverId = drivers[1].Id,
                StartDate = DateTime.UtcNow.AddDays(-45),
                EndDate = DateTime.UtcNow.AddDays(-10),
                Status = AssignmentStatus.Completed,
                Notes = "Assigned to Sarah for long-haul routes",
                CreatedAt = DateTime.UtcNow
            },
            new Assignment
            {
                VehicleId = vehicles[2].Id,
                DriverId = drivers[2].Id,
                StartDate = DateTime.UtcNow.AddDays(-4),
                EndDate = DateTime.UtcNow.AddDays(60),
                Status = AssignmentStatus.Active,
                Notes = "City delivery routes",
                CreatedAt = DateTime.UtcNow
            },
            new Assignment
            {
                VehicleId = vehicles[2].Id,
                DriverId = drivers[0].Id,
                StartDate = DateTime.UtcNow.AddDays(-20),
                EndDate = DateTime.UtcNow.AddDays(-5),
                Status = AssignmentStatus.Completed,
                Notes = "Temporary assignment during peak season",
                CreatedAt = DateTime.UtcNow
            },
            new Assignment
            {
                VehicleId = vehicles[3].Id,
                DriverId = drivers[1].Id,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = AssignmentStatus.Active,
                Notes = "Heavy-duty truck assignments",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Assignments.AddRange(assignments);
        context.SaveChanges();
    }
}
