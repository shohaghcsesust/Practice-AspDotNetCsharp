# Leave Management API - Design Overview

## Table of Contents
1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Design Patterns](#design-patterns)
5. [Database Design](#database-design)
6. [API Endpoints](#api-endpoints)
7. [Data Flow](#data-flow)
8. [Technology Stack](#technology-stack)
9. [Configuration](#configuration)
10. [Getting Started](#getting-started)

---

## Project Overview

The **Leave Management API** is a RESTful web service built with ASP.NET Core that allows organizations to manage employee leave requests. The system supports:

- Employee management (CRUD operations)
- Leave type configuration (Annual, Sick, Casual, etc.)
- Leave request submission, approval, and rejection
- Leave request status tracking

---

## Architecture

The project follows a **Clean/Layered Architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│                        (Controllers)                         │
│  ┌─────────────────┬─────────────────┬─────────────────┐    │
│  │ EmployeesCtrl   │ LeaveTypesCtrl  │ LeaveRequestsCtrl│    │
│  └────────┬────────┴────────┬────────┴────────┬────────┘    │
└───────────┼─────────────────┼─────────────────┼─────────────┘
            │                 │                 │
            ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                      Business Layer                          │
│                        (Services)                            │
│  ┌─────────────────┬─────────────────┬─────────────────┐    │
│  │ EmployeeService │ LeaveTypeService│LeaveRequestSvc  │    │
│  └────────┬────────┴────────┬────────┴────────┬────────┘    │
└───────────┼─────────────────┼─────────────────┼─────────────┘
            │                 │                 │
            ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                      Data Access Layer                       │
│                       (Repositories)                         │
│  ┌─────────────────┬─────────────────┬─────────────────┐    │
│  │ EmployeeRepo    │ LeaveTypeRepo   │ LeaveRequestRepo│    │
│  └────────┬────────┴────────┬────────┴────────┬────────┘    │
└───────────┼─────────────────┼─────────────────┼─────────────┘
            │                 │                 │
            ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                      Database Layer                          │
│                   (Entity Framework Core)                    │
│              ┌─────────────────────────┐                    │
│              │     LeaveDbContext      │                    │
│              └───────────┬─────────────┘                    │
│                          ▼                                   │
│              ┌─────────────────────────┐                    │
│              │   SQL Server / InMemory │                    │
│              └─────────────────────────┘                    │
└─────────────────────────────────────────────────────────────┘
```

---

## Project Structure

```
LeaveManagementApi/
│
├── Controllers/                    # API Endpoints (Presentation Layer)
│   ├── EmployeesController.cs      # Employee CRUD endpoints
│   ├── LeaveTypesController.cs     # Leave Type CRUD endpoints
│   └── LeaveRequestsController.cs  # Leave Request management endpoints
│
├── Services/                       # Business Logic Layer
│   ├── IEmployeeService.cs         # Employee service interface
│   ├── EmployeeService.cs          # Employee service implementation
│   ├── ILeaveTypeService.cs        # Leave Type service interface
│   ├── LeaveTypeService.cs         # Leave Type service implementation
│   ├── ILeaveRequestService.cs     # Leave Request service interface
│   └── LeaveRequestService.cs      # Leave Request service implementation
│
├── Repositories/                   # Data Access Layer
│   ├── IEmployeeRepository.cs      # Employee repository interface
│   ├── EmployeeRepository.cs       # Employee repository implementation
│   ├── ILeaveTypeRepository.cs     # Leave Type repository interface
│   ├── LeaveTypeRepository.cs      # Leave Type repository implementation
│   ├── ILeaveRequestRepository.cs  # Leave Request repository interface
│   └── LeaveRequestRepository.cs   # Leave Request repository implementation
│
├── Models/                         # Domain Entities
│   ├── Employee.cs                 # Employee entity
│   ├── LeaveType.cs                # Leave Type entity
│   └── LeaveRequest.cs             # Leave Request entity + LeaveStatus enum
│
├── DTOs/                           # Data Transfer Objects
│   └── DTOs.cs                     # All DTOs for API requests/responses
│
├── Data/                           # Database Configuration
│   ├── LeaveDbContext.cs           # EF Core DbContext
│   └── DbInitializer.cs            # Seed data initializer
│
├── Program.cs                      # Application entry point & DI configuration
├── appsettings.json                # Application configuration
├── appsettings.Development.json    # Development-specific configuration
└── LeaveManagementApi.csproj       # Project file with dependencies
```

---

## Design Patterns

### 1. Repository Pattern
Abstracts data access logic from business logic.

```csharp
// Interface defines contract
public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> CreateAsync(Employee employee);
    // ...
}

// Implementation handles data access
public class EmployeeRepository : IEmployeeRepository
{
    private readonly LeaveDbContext _context;
    
    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees.ToListAsync();
    }
}
```

### 2. Service Layer Pattern
Encapsulates business logic and validation.

```csharp
public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _repository;
    
    public async Task<ApiResponse<LeaveRequestDto>> CreateAsync(CreateLeaveRequestDto dto)
    {
        // Business validation
        if (dto.EndDate < dto.StartDate)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Invalid dates");
        
        // Check for overlapping requests
        var hasOverlapping = await _repository.HasOverlappingRequestAsync(...);
        if (hasOverlapping)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Overlapping request exists");
        
        // Create if valid
        var request = await _repository.CreateAsync(...);
        return ApiResponse<LeaveRequestDto>.SuccessResponse(MapToDto(request));
    }
}
```

### 3. Dependency Injection (DI)
All dependencies are injected via constructor injection.

```csharp
// Registration in Program.cs
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Usage in Controller
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    
    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;  // Injected by DI container
    }
}
```

### 4. DTO Pattern (Data Transfer Objects)
Separates API contracts from domain models.

```csharp
// Domain Model (internal)
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public DateTime CreatedAt { get; set; }  // Not exposed to API
}

// DTO (external contract)
public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string FullName => $"{FirstName} {LastName}";  // Computed property
}
```

---

## Database Design

### Entity Relationship Diagram

```
┌──────────────────┐       ┌──────────────────┐
│    Employee      │       │    LeaveType     │
├──────────────────┤       ├──────────────────┤
│ Id (PK)          │       │ Id (PK)          │
│ FirstName        │       │ Name             │
│ LastName         │       │ Description      │
│ Email (Unique)   │       │ DefaultDays      │
│ Department       │       │ IsActive         │
│ Position         │       │ CreatedAt        │
│ HireDate         │       │ UpdatedAt        │
│ IsActive         │       └────────┬─────────┘
│ CreatedAt        │                │
│ UpdatedAt        │                │
└────────┬─────────┘                │
         │                          │
         │    ┌─────────────────────┘
         │    │
         ▼    ▼
┌──────────────────────────┐
│      LeaveRequest        │
├──────────────────────────┤
│ Id (PK)                  │
│ EmployeeId (FK)          │──────→ Employee
│ LeaveTypeId (FK)         │──────→ LeaveType
│ StartDate                │
│ EndDate                  │
│ TotalDays                │
│ Reason                   │
│ Status (Enum)            │
│ ApproverComments         │
│ ApprovedById (FK)        │──────→ Employee
│ ApprovedAt               │
│ CreatedAt                │
│ UpdatedAt                │
└──────────────────────────┘
```

### Leave Status Enum

```csharp
public enum LeaveStatus
{
    Pending = 0,    // Awaiting approval
    Approved = 1,   // Approved by manager
    Rejected = 2,   // Rejected by manager
    Cancelled = 3   // Cancelled by employee
}
```

---

## API Endpoints

### Employees API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/employees` | Get all employees |
| `GET` | `/api/employees/{id}` | Get employee by ID |
| `POST` | `/api/employees` | Create new employee |
| `PUT` | `/api/employees/{id}` | Update employee |
| `DELETE` | `/api/employees/{id}` | Delete employee |

### Leave Types API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/leavetypes` | Get all leave types |
| `GET` | `/api/leavetypes/active` | Get active leave types only |
| `GET` | `/api/leavetypes/{id}` | Get leave type by ID |
| `POST` | `/api/leavetypes` | Create new leave type |
| `PUT` | `/api/leavetypes/{id}` | Update leave type |
| `DELETE` | `/api/leavetypes/{id}` | Delete leave type |

### Leave Requests API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/leaverequests` | Get all leave requests |
| `GET` | `/api/leaverequests/pending` | Get pending requests |
| `GET` | `/api/leaverequests/employee/{employeeId}` | Get requests by employee |
| `GET` | `/api/leaverequests/{id}` | Get request by ID |
| `POST` | `/api/leaverequests` | Create new request |
| `PUT` | `/api/leaverequests/{id}` | Update pending request |
| `POST` | `/api/leaverequests/{id}/approve` | Approve request |
| `POST` | `/api/leaverequests/{id}/reject` | Reject request |
| `POST` | `/api/leaverequests/{id}/cancel` | Cancel request |
| `DELETE` | `/api/leaverequests/{id}` | Delete request |

---

## Data Flow

### Example: Creating a Leave Request

```
┌─────────┐     ┌────────────────────┐     ┌─────────────────────┐
│  Client │────▶│ LeaveRequestsCtrl  │────▶│ LeaveRequestService │
└─────────┘     └────────────────────┘     └──────────┬──────────┘
                                                      │
    ┌─────────────────────────────────────────────────┘
    │
    ▼
┌───────────────────────────────────────────────────────────────┐
│                    Validation Steps                           │
├───────────────────────────────────────────────────────────────┤
│ 1. Validate Employee exists (IEmployeeRepository)             │
│ 2. Validate LeaveType exists (ILeaveTypeRepository)           │
│ 3. Validate dates (EndDate >= StartDate)                      │
│ 4. Check for overlapping requests (ILeaveRequestRepository)   │
└───────────────────────────────────────────────────────────────┘
    │
    ▼ (If all validations pass)
┌───────────────────────────────────────────────────────────────┐
│              ILeaveRequestRepository.CreateAsync()            │
└───────────────────────────────────────────────────────────────┘
    │
    ▼
┌───────────────────────────────────────────────────────────────┐
│                  LeaveDbContext.SaveChanges()                 │
└───────────────────────────────────────────────────────────────┘
    │
    ▼
┌───────────────────────────────────────────────────────────────┐
│               Return ApiResponse<LeaveRequestDto>             │
└───────────────────────────────────────────────────────────────┘
```

---

## Technology Stack

| Component | Technology |
|-----------|------------|
| **Framework** | ASP.NET Core (.NET 10) |
| **Language** | C# 12 |
| **ORM** | Entity Framework Core 10 |
| **Database** | SQL Server / In-Memory (configurable) |
| **API Documentation** | OpenAPI 3.0 + Scalar UI |
| **Dependency Injection** | Built-in ASP.NET Core DI |

---

## Configuration

### Database Configuration (Program.cs)

```csharp
// Option 1: In-Memory Database (Development/Demo)
builder.Services.AddDbContext<LeaveDbContext>(options =>
    options.UseInMemoryDatabase("LeaveManagementDb"));

// Option 2: SQL Server (Production)
builder.Services.AddDbContext<LeaveDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LeaveManagementDb;Trusted_Connection=True;"
  }
}
```

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 / VS Code
- SQL Server (optional, for production)

### Run the Application

```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Access API Documentation
# Open browser: http://localhost:5000/scalar/v1
```

### Test API Endpoints

```bash
# Get all employees
curl http://localhost:5000/api/employees

# Create a leave request
curl -X POST http://localhost:5000/api/leaverequests \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": 1,
    "leaveTypeId": 1,
    "startDate": "2024-01-15",
    "endDate": "2024-01-17",
    "reason": "Vacation"
  }'
```

---

## Future Enhancements

- [ ] JWT Authentication & Authorization
- [ ] Role-based access (Admin, Manager, Employee)
- [ ] Leave balance tracking
- [ ] Email notifications
- [ ] Audit logging
- [ ] Pagination for large datasets
- [ ] Unit & Integration tests

---

*Document Version: 1.0*  
*Last Updated: December 2025*
