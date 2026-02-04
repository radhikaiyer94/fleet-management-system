using FleetManagementApi.Domain.Enums;
using FleetManagementApi.Domain.Entities;

namespace FleetManagementApi.Models;

/// <summary>
/// Query parameters for GET /api/vehicles. All properties are optional.
/// Binds from query string: ?search=camry&make=Toyota&year=2022&page=1&pageSize=20
/// </summary>
public class GetVehiclesQuery
{
    private const int DefaultPageSize = 10;
    private const int DefaultPage = 1;

    /// <summary>General text search across Make, Model, VIN, LicensePlate.</summary>
    public string? Search { get; set; }

    /// <summary>Filter by make (contains, case-insensitive).</summary>
    public string? Make { get; set; }

    /// <summary>Filter by model (contains, case-insensitive).</summary>
    public string? Model { get; set; }

    /// <summary>Filter by exact year.</summary>
    public int? Year { get; set; }

    /// <summary>Filter by status (0=Available, 1=NotAvailable, 2=UnderMaintenance).</summary>
    public VehicleStatus? Status { get; set; }

    /// <summary>Filter by VIN (contains, case-insensitive).</summary>
    public string? Vin { get; set; }

    /// <summary>Filter by license plate (contains, case-insensitive).</summary>
    public string? LicensePlate { get; set; }

    /// <summary>Sort order (asc or desc).</summary>
    public string SortOrder { get; set; } = "asc";

    /// <summary>Sort by field (id, make, model, year, status, createdat, vin, licenseplate).</summary>
    public string SortBy { get; set; } = nameof(Vehicle.Id);

    /// <summary>Page number (1-based). Used only when PageSize is set.</summary>
    public int Page { get; set; } = DefaultPage;

    /// <summary>Page size. Used only when Page is set.</summary>
    public int PageSize { get; set; } = DefaultPageSize;
}
