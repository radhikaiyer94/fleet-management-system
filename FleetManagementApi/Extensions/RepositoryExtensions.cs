using FleetManagementApi.Repositories.Interfaces;
using FleetManagementApi.Repositories.Implementation;

namespace FleetManagementApi.Extensions;

public static class RepositoryExtensions
{
    /// <summary>
    /// Registers all repository interfaces and implementations (Scoped).
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IMaintenanceRecordRepository, MaintenanceRecordRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        return services;
    }
}
