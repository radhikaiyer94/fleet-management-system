using System.Text.Json.Serialization;
using FleetManagementApi.Data;
using FleetManagementApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Controllers and API behavior
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

// Swagger with JWT Bearer support
builder.Services.AddSwaggerWithBearer();

// Database (EF Core + PostgreSQL)
builder.Services.AddFleetDatabase(builder.Configuration);

// Repositories and application services
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();

// Exception handling
builder.Services.AddExceptionHandler<FleetManagementApi.Exceptions.ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// JWT authentication and authorization
builder.Services.AddJwtAuthenticationAndAuthorization(builder.Configuration);

var app = builder.Build();

// Apply migrations and seed data
app.UseFleetDatabaseMigrations();

// Pipeline
app.UseExceptionHandler();
app.UseSwaggerInDevelopment();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
