using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Models;

namespace FleetManagementApi.Repositories.Interfaces;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Assignment>> GetAsync(GetAssignmentsQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Assignment>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Assignment> CreateAsync(Assignment assignment, CancellationToken cancellationToken = default);
    Task<Assignment> UpdateAsync(Assignment assignment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> VehicleHasActiveAssignmentAsync(Guid vehicleId, Guid? excludeAssignmentId, CancellationToken cancellationToken = default);
    Task<bool> DriverHasActiveAssignmentAsync(Guid driverId, Guid? excludeAssignmentId, CancellationToken cancellationToken = default);
}
