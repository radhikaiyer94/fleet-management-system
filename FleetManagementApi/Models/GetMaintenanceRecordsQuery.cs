using FleetManagementApi.Domain.Enums;
using FleetManagementApi.Domain.Entities;

namespace FleetManagementApi.Models;

/// <summary>Query parameters for GET /api/maintenancerecords. All properties optional.</summary>
public class GetMaintenanceRecordsQuery
{
    private const int DefaultPageSize = 10;
    private const int DefaultPage = 1;

    /// <summary>Search in Description and ServiceProvider.</summary>
    public string? Search { get; set; }

    /// <summary>Filter by vehicle ID.</summary>
    public Guid? VehicleId { get; set; }

    /// <summary>Filter by maintenance type (0-7).</summary>
    public MaintenanceType? Type { get; set; }

    /// <summary>Sort order (asc or desc).</summary>
    public string SortOrder { get; set; } = "asc";

    /// <summary>Sort by: id, vehicleid, maintenancetype, cost, servicedate, createdat.</summary>
    public string SortBy { get; set; } = nameof(MaintenanceRecord.Id);

    public int Page { get; set; } = DefaultPage;
    public int PageSize { get; set; } = DefaultPageSize;
}
