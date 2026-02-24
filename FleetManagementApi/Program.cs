using Microsoft.EntityFrameworkCore;
using FleetManagementApi.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FleetManagementApi.Domain.Enums;
using FleetManagementApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Service Registration - Dependency Injection Container
// ============================================================================

// Register API controllers for handling HTTP requests
// Configure JSON serialization to handle circular references in navigation properties
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignore circular references (e.g., Vehicle -> MaintenanceRecords -> Vehicle -> ...)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic 400 response for invalid ModelState so our action runs and we
        // can throw BadRequestException → ApiExceptionHandler returns { "message": "..." }.
        options.SuppressModelStateInvalidFilter = true;
    });

// Enable API endpoint exploration for Swagger documentation
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI documentation
// Swagger provides interactive API documentation and testing interface
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Fleet Management API",
        Version = "v1",
        Description = "API for managing fleet vehicles, drivers, and maintenance records"
    });

    // JWT Bearer support: "Authorize" in Swagger UI lets you paste a token; it is sent as Authorization: Bearer <token> on each request
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token (no \"Bearer \" prefix needed).",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================================
// Database Configuration - Entity Framework Core
// ============================================================================
// Register FleetDbContext with PostgreSQL database provider
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=FleetManagementDb;Username=postgres;Password=postgres;";

builder.Services.AddDbContext<FleetDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions
        .MapEnum<UserRole>()
        .MapEnum<VehicleStatus>()
        .MapEnum<DriverStatus>()
        .MapEnum<MaintenanceType>()
        .MapEnum<AssignmentStatus>()));

// Repositories and application services (see Extensions/ for registration details)
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();

// Central exception handling: custom exceptions → HTTP status + JSON { "message": "..." }
builder.Services.AddExceptionHandler<FleetManagementApi.Exceptions.ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey is required.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? string.Empty;
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? string.Empty;

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
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

//Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole(UserRole.Admin.ToString()));
});

var app = builder.Build();

// ============================================================================
// Database Initialization - Run Migrations and Seed Data
// ============================================================================
// Create a scope to access the DbContext
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<FleetDbContext>();

    // Run pending EF Core migrations (e.g. create/update tables for UUID schema)
    // Safe to call on every startup: applies only migrations that have not been applied yet
    context.Database.Migrate();

    // Seed sample data into the database (idempotent - won't duplicate data)
    SeedData.Initialize(context);
}

// ============================================================================
// HTTP Request Pipeline Configuration
// Middleware components are executed in the order they are registered
// ============================================================================

// Central exception handler (must be early so it catches exceptions from controllers)
app.UseExceptionHandler();

// Enable Swagger UI only in Development environment
// Swagger provides interactive API documentation and testing
if (app.Environment.IsDevelopment())
{
    // Enable Swagger JSON endpoint (generates OpenAPI specification)
    app.UseSwagger();
    
    // Configure Swagger UI (interactive API documentation)
    app.UseSwaggerUI(c =>
    {
        // Specify the Swagger JSON endpoint
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fleet Management API v1");
        
        // Set Swagger UI at the application root (/) instead of /swagger
        // This makes it easier to access: http://localhost:5000
        c.RoutePrefix = string.Empty;
    });
}

// Redirect HTTP to HTTPS (skip in Development when only HTTP is used, to avoid "Failed to determine the https port" warning)
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// Authentication (who are you?) then Authorization (what can you do?)
app.UseAuthentication();
// Enable authorization middleware (for authentication/authorization features)
app.UseAuthorization();

// Map controller endpoints to routes
// Controllers will be accessible via their [Route] attributes
app.MapControllers();

// Start the web server and begin listening for HTTP requests
app.Run();

