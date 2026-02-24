using FleetManagementApi.Repositories.Interfaces;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Data;
using FleetManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementApi.Repositories.Implementation;

public class DriverRepository : IDriverRepository
{
    private readonly FleetDbContext _dbContext;

    public DriverRepository(FleetDbContext context)
    {
        _dbContext = context;
    }

    public async Task<Driver?> GetDriverByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Drivers
            .Include(d => d.Assignments.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Driver>> GetDriversAsync(GetDriversQuery q, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Drivers.Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(d =>
                (d.FirstName != null && d.FirstName.ToLower().Contains(term)) ||
                (d.LastName != null && d.LastName.ToLower().Contains(term)) ||
                (d.Email != null && d.Email.ToLower().Contains(term)) ||
                (d.PhoneNumber != null && d.PhoneNumber.ToLower().Contains(term)) ||
                (d.LicenseNumber != null && d.LicenseNumber.ToLower().Contains(term)));
        }
        if (q.Status.HasValue)
            query = query.Where(d => d.Status == q.Status.Value);

        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            var sortByLower = q.SortBy.Trim().ToLower();
            var desc = string.Equals(q.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            query = sortByLower switch
            {
                "id" => desc ? query.OrderByDescending(d => d.Id) : query.OrderBy(d => d.Id),
                "firstname" => desc ? query.OrderByDescending(d => d.FirstName) : query.OrderBy(d => d.FirstName),
                "lastname" => desc ? query.OrderByDescending(d => d.LastName) : query.OrderBy(d => d.LastName),
                "status" => desc ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
                "createdat" => desc ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
                _ => query.OrderBy(d => d.Id)
            };
        }
        else
            query = query.OrderBy(d => d.Id);

        if (q.Page > 0 && q.PageSize > 0)
            query = query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Driver> CreateDriverAsync(Driver driver, CancellationToken cancellationToken = default)
    {
        await _dbContext.Drivers.AddAsync(driver, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return driver;
    }

    public async Task<Driver> UpdateDriverAsync(Driver driver, CancellationToken cancellationToken = default)
    {
        _dbContext.Drivers.Update(driver);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return driver;
    }

    public async Task<bool> DeleteDriverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
        if (driver == null)
            return false;
        driver.IsDeleted = true;
        driver.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<Assignment>> GetAssignmentsByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Assignments
            .Include(a => a.Vehicle)
            .Include(a => a.Driver)
            .Where(a => a.DriverId == driverId && !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}
