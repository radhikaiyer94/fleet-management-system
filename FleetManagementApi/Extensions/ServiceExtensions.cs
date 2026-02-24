using FleetManagementApi.Services;

namespace FleetManagementApi.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Registers application services (Scoped). Depends on repositories being registered.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<VehiclesService>();
        services.AddScoped<DriversService>();
        services.AddScoped<MaintenanceRecordsService>();
        services.AddScoped<AssignmentsService>();
        return services;
    }
}
