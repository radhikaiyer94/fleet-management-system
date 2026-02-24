using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Models;

namespace FleetManagementApi.Repositories.Interfaces;

public interface IDriverRepository
{
    Task<Driver?> GetDriverByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Driver>> GetDriversAsync(GetDriversQuery query, CancellationToken cancellationToken = default);
    Task<Driver> CreateDriverAsync(Driver driver, CancellationToken cancellationToken = default);
    Task<Driver> UpdateDriverAsync(Driver driver, CancellationToken cancellationToken = default);
    Task<bool> DeleteDriverAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Assignment>> GetAssignmentsByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
}
