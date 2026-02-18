using System.ComponentModel.DataAnnotations;
using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;


    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}