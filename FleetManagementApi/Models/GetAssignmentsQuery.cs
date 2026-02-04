using FleetManagementApi.Domain.Enums;
using FleetManagementApi.Domain.Entities;

namespace FleetManagementApi.Models;

/// <summary>Query parameters for GET /api/assignments. All properties optional.</summary>
public class GetAssignmentsQuery
{
    private const int DefaultPageSize = 10;
    private const int DefaultPage = 1;

    /// <summary>Search in Notes.</summary>
    public string? Search { get; set; }

    /// <summary>Filter by vehicle ID.</summary>
    public Guid? VehicleId { get; set; }

    /// <summary>Filter by driver ID.</summary>
    public Guid? DriverId { get; set; }

    /// <summary>Filter by status (0=Active, 1=Completed, 2=Cancelled).</summary>
    public AssignmentStatus? Status { get; set; }

    /// <summary>Sort order (asc or desc).</summary>
    public string SortOrder { get; set; } = "asc";

    /// <summary>Sort by: id, vehicleid, driverid, startdate, enddate, status, createdat.</summary>
    public string SortBy { get; set; } = nameof(Assignment.Id);

    public int Page { get; set; } = DefaultPage;
    public int PageSize { get; set; } = DefaultPageSize;
}
