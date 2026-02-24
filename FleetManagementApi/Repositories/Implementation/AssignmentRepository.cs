using FleetManagementApi.Repositories.Interfaces;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Domain.Enums;
using FleetManagementApi.Data;
using FleetManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementApi.Repositories.Implementation;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly FleetDbContext _dbContext;

    public AssignmentRepository(FleetDbContext context)
    {
        _dbContext = context;
    }

    public async Task<Assignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Assignments
            .Include(a => a.Vehicle)
            .Include(a => a.Driver)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Assignment>> GetAsync(GetAssignmentsQuery q, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Assignments
            .Include(a => a.Vehicle)
            .Include(a => a.Driver)
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(a => a.Notes != null && a.Notes.ToLower().Contains(term));
        }
        if (q.VehicleId.HasValue)
            query = query.Where(a => a.VehicleId == q.VehicleId.Value);
        if (q.DriverId.HasValue)
            query = query.Where(a => a.DriverId == q.DriverId.Value);
        if (q.Status.HasValue)
            query = query.Where(a => a.Status == q.Status.Value);

        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            var sortByLower = q.SortBy.Trim().ToLower();
            var desc = string.Equals(q.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            query = sortByLower switch
            {
                "id" => desc ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id),
                "vehicleid" => desc ? query.OrderByDescending(a => a.VehicleId) : query.OrderBy(a => a.VehicleId),
                "driverid" => desc ? query.OrderByDescending(a => a.DriverId) : query.OrderBy(a => a.DriverId),
                "startdate" => desc ? query.OrderByDescending(a => a.StartDate) : query.OrderBy(a => a.StartDate),
                "enddate" => desc ? query.OrderByDescending(a => a.EndDate) : query.OrderBy(a => a.EndDate),
                "status" => desc ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                "createdat" => desc ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
                _ => query.OrderBy(a => a.Id)
            };
        }
        else
            query = query.OrderBy(a => a.Id);

        if (q.Page > 0 && q.PageSize > 0)
            query = query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Assignment>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Assignments
            .Include(a => a.Vehicle)
            .Include(a => a.Driver)
            .Where(a => !a.IsDeleted && a.Status == AssignmentStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<Assignment> CreateAsync(Assignment assignment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Assignments.AddAsync(assignment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<Assignment> UpdateAsync(Assignment assignment, CancellationToken cancellationToken = default)
    {
        _dbContext.Assignments.Update(assignment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _dbContext.Assignments.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
        if (assignment == null)
            return false;
        assignment.IsDeleted = true;
        assignment.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> VehicleHasActiveAssignmentAsync(Guid vehicleId, Guid? excludeAssignmentId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Assignments
            .Where(a => a.VehicleId == vehicleId && a.Status == AssignmentStatus.Active && !a.IsDeleted);
        if (excludeAssignmentId.HasValue)
            query = query.Where(a => a.Id != excludeAssignmentId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> DriverHasActiveAssignmentAsync(Guid driverId, Guid? excludeAssignmentId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Assignments
            .Where(a => a.DriverId == driverId && a.Status == AssignmentStatus.Active && !a.IsDeleted);
        if (excludeAssignmentId.HasValue)
            query = query.Where(a => a.Id != excludeAssignmentId.Value);
        return await query.AnyAsync(cancellationToken);
    }
}
