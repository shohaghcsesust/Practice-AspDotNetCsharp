using LeaveManagementApi.Data;
using LeaveManagementApi.Repositories;
using LeaveManagementApi.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework with In-Memory Database (for demo)
// For production, use SQL Server:
// builder.Services.AddDbContext<LeaveDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<LeaveDbContext>(options =>
    options.UseInMemoryDatabase("LeaveManagementDb"));

// Register Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();

// Register Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();

// Add OpenAPI/Swagger support
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
    DbInitializer.Initialize(context);
}

// Configure the HTTP request pipeline.
// Map OpenAPI endpoint
app.MapOpenApi();

// Enable Scalar API documentation UI
app.MapScalarApiReference(options =>
{
    options.WithTitle("Leave Management API");
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
