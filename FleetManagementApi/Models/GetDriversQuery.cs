using FleetManagementApi.Domain.Enums;
using FleetManagementApi.Domain.Entities;

namespace FleetManagementApi.Models;

/// <summary>Query parameters for GET /api/drivers. All properties optional.</summary>
public class GetDriversQuery
{
    private const int DefaultPageSize = 10;
    private const int DefaultPage = 1;

    /// <summary>Search across FirstName, LastName, Email, PhoneNumber, LicenseNumber.</summary>
    public string? Search { get; set; }

    /// <summary>Filter by status (0=Active, 1=Inactive, 2=Suspended).</summary>
    public DriverStatus? Status { get; set; }

    /// <summary>Sort order (asc or desc).</summary>
    public string SortOrder { get; set; } = "asc";

    /// <summary>Sort by: id, firstname, lastname, status, createdat.</summary>
    public string SortBy { get; set; } = nameof(Driver.Id);

    public int Page { get; set; } = DefaultPage;
    public int PageSize { get; set; } = DefaultPageSize;
}
