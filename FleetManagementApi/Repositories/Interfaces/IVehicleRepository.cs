using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Models;

namespace FleetManagementApi.Repositories.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(GetVehiclesQuery query, CancellationToken cancellationToken = default);

    Task<Vehicle> CreateVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

    Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

    Task<bool> DeleteVehicleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaintenanceRecord>> GetMaintenanceRecordsByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Assignment>> GetAssignmentsByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}