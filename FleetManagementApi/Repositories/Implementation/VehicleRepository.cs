using FleetManagementApi.Repositories.Interfaces;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Data;
using FleetManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementApi.Repositories.Implementation;

public class VehicleRepository : IVehicleRepository
{
    private readonly FleetDbContext _dbContext;

    public VehicleRepository(FleetDbContext context)
    {
        _dbContext = context;
    }

    public async Task<Vehicle?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vehicles.Where(v => !v.IsDeleted).OrderBy(v => v.Id).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(GetVehiclesQuery q, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Vehicles.Where(v => !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(v =>
                (v.Make != null && v.Make.ToLower().Contains(term)) ||
                (v.Model != null && v.Model.ToLower().Contains(term)) ||
                (v.VIN != null && v.VIN.ToLower().Contains(term)) ||
                (v.LicensePlate != null && v.LicensePlate.ToLower().Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(q.Make))
        {
            var makeTerm = q.Make.Trim().ToLower();
            query = query.Where(v => v.Make != null && v.Make.ToLower().Contains(makeTerm));
        }
        if (!string.IsNullOrWhiteSpace(q.Model))
        {
            var modelTerm = q.Model.Trim().ToLower();
            query = query.Where(v => v.Model != null && v.Model.ToLower().Contains(modelTerm));
        }
        if (q.Year.HasValue)
            query = query.Where(v => v.Year == q.Year.Value);
        if (q.Status.HasValue)
            query = query.Where(v => v.Status == q.Status.Value);
        if (!string.IsNullOrWhiteSpace(q.Vin))
        {
            var vinTerm = q.Vin.Trim().ToLower();
            query = query.Where(v => v.VIN != null && v.VIN.ToLower().Contains(vinTerm));
        }
        if (!string.IsNullOrWhiteSpace(q.LicensePlate))
        {
            var plateTerm = q.LicensePlate.Trim().ToLower();
            query = query.Where(v => v.LicensePlate != null && v.LicensePlate.ToLower().Contains(plateTerm));
        }

        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            var sortByLower = q.SortBy.Trim().ToLower();
            var desc = string.Equals(q.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
            query = sortByLower switch
            {
                "id" => desc ? query.OrderByDescending(v => v.Id) : query.OrderBy(v => v.Id),
                "make" => desc ? query.OrderByDescending(v => v.Make) : query.OrderBy(v => v.Make),
                "model" => desc ? query.OrderByDescending(v => v.Model) : query.OrderBy(v => v.Model),
                "year" => desc ? query.OrderByDescending(v => v.Year) : query.OrderBy(v => v.Year),
                "status" => desc ? query.OrderByDescending(v => v.Status) : query.OrderBy(v => v.Status),
                "createdat" => desc ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt),
                "vin" => desc ? query.OrderByDescending(v => v.VIN) : query.OrderBy(v => v.VIN),
                "licenseplate" => desc ? query.OrderByDescending(v => v.LicensePlate) : query.OrderBy(v => v.LicensePlate),
                _ => query.OrderBy(v => v.Id)
            };
        }
        else
            query = query.OrderBy(v => v.Id);

        if (q.Page > 0 && q.PageSize > 0)
            query = query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        await _dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return vehicle;
    }

    public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        _dbContext.Vehicles.Update(vehicle);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return vehicle;
    }

    public async Task<bool> DeleteVehicleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);
        if (vehicle == null)
            return false;
        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<MaintenanceRecord>> GetMaintenanceRecordsByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MaintenanceRecords
            .Where(m => m.VehicleId == vehicleId && !m.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Assignment>> GetAssignmentsByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Assignments
            .Where(a => a.VehicleId == vehicleId && !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}