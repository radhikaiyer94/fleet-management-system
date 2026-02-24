using Microsoft.OpenApi.Models;

namespace FleetManagementApi.Extensions;

public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger/OpenAPI with JWT Bearer security definition and requirement.
    /// </summary>
    public static IServiceCollection AddSwaggerWithBearer(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Fleet Management API",
                Version = "v1",
                Description = "API for managing fleet vehicles, drivers, assignments and maintenance records"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter your token (no \"Bearer \" prefix needed).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }

    /// <summary>
    /// Enables Swagger and Swagger UI when running in Development. Sets Swagger UI at app root (/).
    /// </summary>
    public static IApplicationBuilder UseSwaggerInDevelopment(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        if (!env.IsDevelopment())
            return app;

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fleet Management API v1");
            c.RoutePrefix = string.Empty;
        });
        return app;
    }
}
