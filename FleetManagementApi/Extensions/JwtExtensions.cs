using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FleetManagementApi.Domain.Enums;

namespace FleetManagementApi.Extensions;

public static class JwtExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication and authorization (including Admin policy).
    /// Requires Jwt:SecretKey, Jwt:Issuer, Jwt:Audience in configuration.
    /// </summary>
    public static IServiceCollection AddJwtAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is required.");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? string.Empty;
        var jwtAudience = configuration["Jwt:Audience"] ?? string.Empty;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = key,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole(UserRole.Admin.ToString()));
        });

        return services;
    }
}
