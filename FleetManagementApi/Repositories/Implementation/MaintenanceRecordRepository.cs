using FleetManagementApi.Repositories.Interfaces;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Data;
using FleetManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementApi.Repositories.Implementation;

public class MaintenanceRecordRepository : IMaintenanceRecordRepository
{
    private readonly FleetDbContext _dbContext;

    public MaintenanceRecordRepository(FleetDbContext context)
    {
        _dbContext = context;
    }

    public async Task<MaintenanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MaintenanceRecords
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<MaintenanceRecord>> GetAsync(GetMaintenanceRecordsQuery q, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MaintenanceRecords
            .Include(r => r.Vehicle)
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(r =>
                (r.Description != null && r.Description.ToLower().Contains(term)) ||
                (r.ServiceProvider != null && r.ServiceProvider.ToLower().Contains(term)));
        }
        if (q.VehicleId.HasValue)
            query = query.Where(r => r.VehicleId == q.VehicleId.Value);
        if (q.Type.HasValue)
            query = query.Where(r => r.MaintenanceType == q.Type.Value);

        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            var sortByLower = q.SortBy.Trim().ToLower();
            var desc = string.Equals(q.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            query = sortByLower switch
            {
                "id" => desc ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id),
                "vehicleid" => desc ? query.OrderByDescending(r => r.VehicleId) : query.OrderBy(r => r.VehicleId),
                "maintenancetype" => desc ? query.OrderByDescending(r => r.MaintenanceType) : query.OrderBy(r => r.MaintenanceType),
                "cost" => desc ? query.OrderByDescending(r => r.Cost) : query.OrderBy(r => r.Cost),
                "servicedate" => desc ? query.OrderByDescending(r => r.ServiceDate) : query.OrderBy(r => r.ServiceDate),
                "createdat" => desc ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
                _ => query.OrderBy(r => r.Id)
            };
        }
        else
            query = query.OrderBy(r => r.Id);

        if (q.Page > 0 && q.PageSize > 0)
            query = query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceRecord> CreateAsync(MaintenanceRecord record, CancellationToken cancellationToken = default)
    {
        await _dbContext.MaintenanceRecords.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task<MaintenanceRecord> UpdateAsync(MaintenanceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.MaintenanceRecords.Update(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.MaintenanceRecords.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
        if (record == null)
            return false;
        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
