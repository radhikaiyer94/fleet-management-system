using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Models;

namespace FleetManagementApi.Repositories.Interfaces;

public interface IMaintenanceRecordRepository
{
    Task<MaintenanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceRecord>> GetAsync(GetMaintenanceRecordsQuery query, CancellationToken cancellationToken = default);
    Task<MaintenanceRecord> CreateAsync(MaintenanceRecord record, CancellationToken cancellationToken = default);
    Task<MaintenanceRecord> UpdateAsync(MaintenanceRecord record, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
