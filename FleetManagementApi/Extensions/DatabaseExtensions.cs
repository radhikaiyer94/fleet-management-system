using Microsoft.EntityFrameworkCore;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// Registers FleetDbContext with PostgreSQL and maps all domain enums to PostgreSQL enum types.
    /// </summary>
    public static IServiceCollection AddFleetDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=FleetManagementDb;Username=postgres;Password=postgres;";

        services.AddDbContext<FleetDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions
                .MapEnum<UserRole>()
                .MapEnum<VehicleStatus>()
                .MapEnum<DriverStatus>()
                .MapEnum<MaintenanceType>()
                .MapEnum<AssignmentStatus>()));

        return services;
    }

    /// <summary>
    /// Applies pending EF Core migrations and runs seed data. Call once at startup.
    /// </summary>
    public static IApplicationBuilder UseFleetDatabaseMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
        context.Database.Migrate();
        SeedData.Initialize(context);
        return app;
    }
}
