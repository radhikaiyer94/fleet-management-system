using FleetManagementApi.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class MapStatusEnumsToPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:assignment_status", "active,cancelled,completed")
                .Annotation("Npgsql:Enum:driver_status", "active,inactive,suspended")
                .Annotation("Npgsql:Enum:maintenance_type", "brake_service,cleaning,inspection,oil_change,other,repair,tire_replacement,washing")
                .Annotation("Npgsql:Enum:user_role", "admin,driver,fleet_manager")
                .Annotation("Npgsql:Enum:vehicle_status", "available,not_available,under_maintenance")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,driver,fleet_manager");

            // Convert integer columns to PostgreSQL enums with USING (required when column had integer values)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Vehicles"" ALTER COLUMN ""Status"" TYPE vehicle_status
                USING (CASE ""Status"" WHEN 0 THEN 'available'::vehicle_status WHEN 1 THEN 'not_available'::vehicle_status WHEN 2 THEN 'under_maintenance'::vehicle_status END);
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE ""MaintenanceRecords"" ALTER COLUMN ""MaintenanceType"" TYPE maintenance_type
                USING (CASE ""MaintenanceType"" WHEN 0 THEN 'cleaning'::maintenance_type WHEN 1 THEN 'washing'::maintenance_type WHEN 2 THEN 'oil_change'::maintenance_type WHEN 3 THEN 'tire_replacement'::maintenance_type WHEN 4 THEN 'brake_service'::maintenance_type WHEN 5 THEN 'inspection'::maintenance_type WHEN 6 THEN 'repair'::maintenance_type WHEN 7 THEN 'other'::maintenance_type END);
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE ""Drivers"" ALTER COLUMN ""Status"" TYPE driver_status
                USING (CASE ""Status"" WHEN 0 THEN 'active'::driver_status WHEN 1 THEN 'inactive'::driver_status WHEN 2 THEN 'suspended'::driver_status END);
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE ""Assignments"" ALTER COLUMN ""Status"" TYPE assignment_status
                USING (CASE ""Status"" WHEN 0 THEN 'active'::assignment_status WHEN 1 THEN 'completed'::assignment_status WHEN 2 THEN 'cancelled'::assignment_status END);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:user_role", "admin,driver,fleet_manager")
                .OldAnnotation("Npgsql:Enum:assignment_status", "active,cancelled,completed")
                .OldAnnotation("Npgsql:Enum:driver_status", "active,inactive,suspended")
                .OldAnnotation("Npgsql:Enum:maintenance_type", "brake_service,cleaning,inspection,oil_change,other,repair,tire_replacement,washing")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,driver,fleet_manager")
                .OldAnnotation("Npgsql:Enum:vehicle_status", "available,not_available,under_maintenance");

            // Revert enum columns back to integer
            migrationBuilder.Sql(@"ALTER TABLE ""Vehicles"" ALTER COLUMN ""Status"" TYPE integer USING (CASE ""Status""::text WHEN 'available' THEN 0 WHEN 'not_available' THEN 1 WHEN 'under_maintenance' THEN 2 END);");
            migrationBuilder.Sql(@"ALTER TABLE ""MaintenanceRecords"" ALTER COLUMN ""MaintenanceType"" TYPE integer USING (CASE ""MaintenanceType""::text WHEN 'cleaning' THEN 0 WHEN 'washing' THEN 1 WHEN 'oil_change' THEN 2 WHEN 'tire_replacement' THEN 3 WHEN 'brake_service' THEN 4 WHEN 'inspection' THEN 5 WHEN 'repair' THEN 6 WHEN 'other' THEN 7 END);");
            migrationBuilder.Sql(@"ALTER TABLE ""Drivers"" ALTER COLUMN ""Status"" TYPE integer USING (CASE ""Status""::text WHEN 'active' THEN 0 WHEN 'inactive' THEN 1 WHEN 'suspended' THEN 2 END);");
            migrationBuilder.Sql(@"ALTER TABLE ""Assignments"" ALTER COLUMN ""Status"" TYPE integer USING (CASE ""Status""::text WHEN 'active' THEN 0 WHEN 'completed' THEN 1 WHEN 'cancelled' THEN 2 END);");
        }
    }
}
