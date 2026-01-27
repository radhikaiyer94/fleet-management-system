using Microsoft.EntityFrameworkCore;
using FleetManagementApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Service Registration - Dependency Injection Container
// ============================================================================

// Register API controllers for handling HTTP requests
builder.Services.AddControllers();

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
});

// ============================================================================
// Database Configuration - Entity Framework Core
// ============================================================================
// Register FleetDbContext with PostgreSQL database provider
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=FleetManagementDb;Username=postgres;Password=postgres;";

builder.Services.AddDbContext<FleetDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// ============================================================================
// Database Initialization - Seed Sample Data
// ============================================================================
// Create a scope to access the DbContext and seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<FleetDbContext>();
    
    // Seed sample data into the database (idempotent - won't duplicate data)
    SeedData.Initialize(context);
}

// ============================================================================
// HTTP Request Pipeline Configuration
// Middleware components are executed in the order they are registered
// ============================================================================

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

// Redirect HTTP requests to HTTPS (security best practice)
app.UseHttpsRedirection();

// Enable authorization middleware (for future authentication/authorization features)
app.UseAuthorization();

// Map controller endpoints to routes
// Controllers will be accessible via their [Route] attributes
app.MapControllers();

// Start the web server and begin listening for HTTP requests
app.Run();

